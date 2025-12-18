# FPS RobotShooter


## 1. 프로젝트 개요

- **프로젝트명**: FPS_RobotShooter
- **개발 기간**: [2025.10 ~ ] (MVP단계 완료)
- **수행 방식**: **개인 프로젝트 (1인 개발)**
- **담당 범위: 전체**
- **개발 현황**: 개발 진행중 (미출시)
    - GitHub: https://github.com/Mephilos/Unity_RobotShooter
- **기술 스택**: Unity3D URP, UGUI, VSCode, GitHub

---

## 2. 기획 의도 및 목표

### 개발 동기

- 기존의 FPS 에임 트레이닝 게임들의 단조로움에서 벗어나 트레이닝 게임을 재미있고, 도전욕구를 자극, 그리고 비교적 예측하기 어려운 동선, 행동을 적에게 부여하여 PvP FPS게임에서의 교전 상황을 모사 할 수 있는 게임을 목표로 개발하였습니다.

### 게임 개요

- 캐주얼 건 슈팅 게임
- 무기 종류 별로 다른 반동, 탄 퍼짐 시스템
- 적의 타입마다 다른 특징 부여
- 명중률, 클리어 시간, 약점 공격 시 보너스 점수

### 개발 목표

- 플레이어의 도전욕구 자극을 위한 스코어 보드 시스템
    - 명중률, 순위, 클리어 시간, 점수, 백분위 표기
- 범용적인 FPS 게임의 무기특징을 가진 무기의 구현과 바리에이션
- 적들이 상황에 따라 다른 행동을 하여 쉽게 적을 격파 할 수 없는 시스템
- 적에게 약점을 부여하여 약점 타격시 데미지 보너스, 약점 킬시 스코어 보너스를 부여

### 기술적 목표

- 백엔드 연동
- 유니티6의 새로운 인풋 시스템 활용

---


## 3. 기술 구현 및 아키텍처

### 1. 현상황을 고려하여 전술을 선택 하는 상태 패턴 AI 설계

- 구현 이유
    - 단순 공격, 추적을 하는 적이 아닌, 플레이어의 거리, 체력 상황에 따라 적 AI의 행동을 다르게 하여 단조로운 타켓의 움직임에서 벗어나 체력이 유리하면 플레이어를 압박하고, 불리하면 엄폐 도망을 고려하는 타켓이 아닌 적같은 움직임을 구현하기 위해서 설계하였습니다.
- 상세
    1. **상태 패턴 설계** 
        1. EnemyBrain을 Context로 두고 베이스 추상 클래스 BaseEnemyState을 상속하여 적의 행동들을 상태 클래스 PatrolState, SearchState, RangeCombat, GrenadeThrow로 작성하고 캡슐화 하였습니다.
        2. OCP를 고려하여 상태전환 로직 함수인 ChangeState와 행동로직 함수 Execute와 코루틴을 분리하여 새로운 상태 추가만 하면 되도록 하였습니다.
        - 관련 코드 스니펫
            
            ```csharp
            // BaseEnemyState.cs 추상 클래스로 공통 인터페이스 정의
            public abstract class BaseEnemyState
            {
                protected EnemyBrain brain;
                public abstract void Enter();
                public abstract void Execute();
                public abstract void Exit();
            }
            
            // [EnemyBrain.cs] 상태 전환 및 실행 관리 (Context)
            public abstract class EnemyBrain : Enemy
            {
                protected BaseEnemyState currentState;
            
                protected override void Update()
                {
                    // 생략
                    currentState?.Execute(); // 현재 상태의 행동 프래임마다 실행
                }
            
                public void ChangeState(BaseEnemyState newEnemyState)
                {
                    if (currentState == newEnemyState) return;
                    
                    currentState?.Exit();       // 이전 상태 종료
                    currentState = newEnemyState; // 새로운 상태 추가
                    currentState?.Enter();      // 새로운 상태 진입
                }
            }
            ```
            
    2. **상황에 따라 달라지는 적의 전략**
        1. 적은 상태 판단 함수를 통해 상황을 판단합니다
            1. 체력 비교: 우위 동등 불리
            2. 거리 비교: 현재 거리와 무기 사거리를 기준으로 유리 불리
        2. 이러한 판단 기준으로 동등, 유리 사정권안, 유리 사정권밖, 불리 사정권안, 불리 사정권밖의 5개의 행동로직으로 분기하여 작동합니다. 
        - 관련 코드 스니펫
            
            ```csharp
            // RangeEnemy.cs
            public virtual Strategy DetermineStrategy()
            {
            		// 채력, 거리 채크 변수들
             
                // 체력 상황 비교 (동등 열세 우세)
                if (Mathf.Abs(myHP - playerHP) <= 40) return Strategy.Equal;
                else if (myHP < playerHP)
                {
                    // 거리 상황과 조합하여 전략 도출
                    // 체력이 적은데 멀리 있으면 -> 멀리 도망
                    // 체력이 적은데 가까이 있으면 -> 제자리에서 반격
                    return (dist > combatDistance) ? Strategy.DisAdvFar : Strategy.DisAdvNear;
                }
                else
                {
                    // 우세할 경우 -> 공격하며 거리 좁히기 또는 유리한 포지션으로 좁히며 공격
                    return (dist > combatDistance) ? Strategy.AdvFar : Strategy.AdvNear;
                }
            }
            ```
            
    3. **분기된 행동전략에 의거한 엄폐물 탐색**
        1. 전투 루틴중에는 단순히 가까운 엄페물이 아닌 현재 전략에 따른 엄폐물을 찾도록 설계 하였습니다.
            1. Physics.OverlapSphereNonAlloc를 사용 하여 주변을 탐색
            2. 전략에 따라 감지된 엄폐물을 필터링 합니다.
            3. 필터링 된 엄폐물에 의거하여 NavMesh.SamplePosition을 이용하여 검증한뒤 이동을 실행합니다.
        - 관련 코드 스니펫
            
            ```csharp
            // EnemyTactic.cs
            public Vector3 FindCover
            	(Transform playerPosition, Covering action, float searchRadius = 20f)
            {
                // 가비지 컬렉션 방지를 위해 NonAlloc 함수 사용
                int cnt = Physics.OverlapSphereNonAlloc
            		    (transform.position, searchRadius, coverColliders, coverLayer);
                if (cnt == 0) return Vector3.zero;
            
                // 유효한 엄폐물 필터링
            
                // 전략에 따른 엄폐물 우선순위 정렬
                switch (action)
                {
                    case Covering.Near:      
                    case Covering.FarPlayer:  
                    case Covering.NearPlayer:
                }
            
                // 정렬된 엄폐물 중 NavMesh 위에서 이동 가능한 위치인지 검증
                for (int i = 0; i < checkkedCovers.Count; i++)
                {
                    // 은폐 방향 계산
                    if (NavMesh.SamplePosition
            	        (hidePos, out NavMeshHit hit, distToCover, NavMesh.AllAreas))
                    {
                        return hit.position; // 유효한 엄폐 위치 반환
                    }
                }
                return transform.position;
            }
            ```
            
    4. **코루틴 기반 비동기 전투 제어**
        1. RangeCombatState는 코루틴을 이용하여 전투 루틴을 관리하여 행동단위로 상태 판단 제어를 했습니다.
        - 관련 코드 스니펫
            
            ```csharp
            // RangeCombatState.cs
            protected virtual IEnumerator CombatRoutine()
            {
                // 현재 상황에 맞는 전략 판단
                RangeEnemy.Strategy strategy = range.DetermineStrategy();
                Vector3 coveringPosition = Vector3.zero;
            
                // 전략에 따른 이동 엄폐물 선정
                switch (strategy)
                {
                    case RangeEnemy.Strategy.DisAdvFar: // 불리하면 플레이어 반대편 멀리 엄폐
                        coveringPosition = range.Tactic.
            	           FindCover(playerTransform, EnemyTactic.Covering.FarPlayer);
                        break;
                    case RangeEnemy.Strategy.AdvFar:    // 유리하면 플레이어 근처 엄폐물로 이동하며 압박
                        coveringPosition = range.Tactic.
            	            FindCover(playerTransform, EnemyTactic.Covering.NearPlayer);
                        break;
                    // 기타 케이스 생략
                }
            
                // 이동 전투 액션 실행파트
                if (coveringPosition != Vector3.zero) 
            	    range.Agent.SetDestination(coveringPosition);
            
                float actionTimer = 0f;
                while (actionTimer < 2.0f) // 일정 시간 동안 전략 수행
                {
                    // 사격, 애니메이션, 회전 로직
                    yield return null;
                }
                
                range.OnCombatFinish(); // 행동 종료 후 다음 상태로 전환
            }
            ```
            
        
        ![제목 없는 다이어그램.png](FPS%20RobotShooter/%E1%84%8C%E1%85%A6%E1%84%86%E1%85%A9%E1%86%A8_%E1%84%8B%E1%85%A5%E1%86%B9%E1%84%82%E1%85%B3%E1%86%AB_%E1%84%83%E1%85%A1%E1%84%8B%E1%85%B5%E1%84%8B%E1%85%A5%E1%84%80%E1%85%B3%E1%84%85%E1%85%A2%E1%86%B7.png)
        

### 2. Firebase 기반 비동기 백엔드 및 실시간 리더보드 시스템

- 구현 이유
    
    스테이지별 및 종합 랭킹을 제공해 플레이어의 자연스러운 기록 갱신의 도전 욕구를 고취, 익명 로그인에서 정식 계정으로 확장 가능성도 고려하여 설계하였습니다.
    
- 상세
    
    **1. 이벤트 기반 인증 구조 설계**
    
    - Firebase 인증을 단순 호출 방식이 아닌 이벤트 기반 구조로 설계했습니다.
        
        AuthManager는 로그인/로그아웃 상태를 감지하면 OnLoginSuccess, OnLogout 이벤트를 발생시키고, UI는 이 이벤트를 구독하는 방식으로 동작합니다.
        
        이러한 이벤트 설계로 Firebase와 게임 간의 결합도를 낮추고 향후 인증 방식이 추가되더라도 기존 코드는 온존 한 상태에서 추가 할 수 있도록 하였습니다.
        
        그리고 접근성을 위해 익명 로그인과 로그인 환경이 변하더라도 게임 기록을 유지 할 수 있는 이메일 로그인도 지원 하도록 설계하였습니다.
        
    - 관련 코드 스니펫
        
        ```csharp
        // AuthManager.cs
        public class AuthManager : MonoBehaviour
        {
            // 외부에서 구독할 이벤트 정의
            public event Action<Firebase.Auth.FirebaseUser> OnLoginSuccess;
            public event Action OnLogout;
        
            void AuthStateChanged(object sender, System.EventArgs eventArgs)
            {
                if (firebaseAuth.CurrentUser != firebaseUser)
                {
                    firebaseUser = firebaseAuth.CurrentUser;
                    if (firebaseUser != null)
                    {
                        // 로그인 감지 시 이벤트 발생
                        OnLoginSuccess?.Invoke(firebaseUser);
                    }
                    else
                    {
                        // 로그아웃 감지
                        OnLogout?.Invoke();
                    }
                }
            }
            
            // 익명 로그인 지원
            void SignInAnonymously()
            {
                firebaseAuth.SignInAnonymouslyAsync().
        	        ContinueWithOnMainThread(task => { // 생략});
            }
        }
        ```
        
    
    **2. Task 기반 비동기 네트워크 처리**
    
    - 로그인, 점수 저장, 데이터 로드 등 모든 네트워크 i/o는 Task메서드로 처리해 프리징이 발생하지 않도록 했습니다.
        
        Unity API는 메인 스레드에서만 접근 가능하므로, Firebase의 ContinueWithOnMainThread를 사용해 스레드 컨텍스트  불일치 문제를 방지하고, 콜백이 중첩되지 않게 하여 가독성을 높였습니다.
        
    - 관련 코드 스니펫
        
        ```csharp
        // FirebaseManager.cs
        public void LoadLeaderboardData(int stageIndex, Action<List<UserScoreData>> onLoad)
        {
            // Task 비동기 처리로 메인 스레드 프리징 방지
            databaseReference.Child("users").OrderByChild(scorePath).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    return; // 예외 처리
                }
                DataSnapshot snapshot = task.Result; // 메인 스레드 컨텍스트 보장 UI
                // 데이터 파싱 로직
                onLoad?.Invoke(rankList);
            });
        }
        ```
        
    
    **3. 점수 저장 최적화, 저장시 데이터 동기화**
    
    - 점수 저장 시 매번 덮어쓸때의 리소스 낭비를 막기위해 기존의 최고 기록과 현재 기록을 비교해 고점 갱신 시에만 서버에 갱신을 요청하도록 최적화 하였습니다.
        
        그리고 닉네임 변경시 점수 갱신이 함께 이루어지도록 UpdateChildrenAsync을 사용 하여 한번의 요청으로 묶어서 처리하도록 하였습니다.
        
        이로써 네트워크 리소스는 줄이고 둘중하나만 저장되는 경우를 없게 하여 데이터가 유실 되더라도 최소한 데이터가 꼬이는 일이 없도록 하였습니다.
        
    - 관련 코드 스니펫
        
        ```csharp
        // FirebaseManager.cs
        public void StageScoreSave(int stageIndex, int currentScore, float currentTime, /*...*/)
        {
            // dbBestScore 디비에 저장된 최고 점수 조회 로직
        
            // 최적화: 신기록일 경우에만 요청
            if (currentScore > dbBestScore)
            {
                // 닉네임과 점수를 묶어서 처리 (유실될지언정 꼬여서 데이터베이스 더럽히지 않게)
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    ["userName"] = userName,
                    ["score"] = currentScore,
                    ["time"] = currentTime,
                    ["acc"] = currentAcc
                };
        
                // UpdateChildrenAsync로 한번에 업데이트
                stageDataRef.UpdateChildrenAsync(updates);
            }
        }
        ```
        
    
    **4. Firebase DB 구조 설계, 리더 보드 최적화**
    
    - 데이터 조회 리소스를 줄이기 위하여 유저→(유저 데이터) 스테이지 → (스테이지별 데이터)로 계층을 나누어 DB를 설계 하였고 유저가 리더보드 데이터 갱신을 요청할 시 유저(클라이언트)가 아닌 서버에서 정렬된 데이터를 받아올 수 있도록 하여 클라이언트, 네크워크 리소스를 절약할수 있게 하였습니다.
    - 관련 코드 스니펫
        
        ```csharp
        // FirebaseManager.cs
        void LoadLeaderboardData(int stageIndex, Action<List<UserScoreData>> onLoad)
        {
            // DB 구조 users -> {users 데이터} stages -> {stageIndex} -> score
            string stageScorePath = (stageIndex == 0) ? "score" : $"stages/{stageIndex}/score";
        
            // 클라이언트 정렬 대신 Firebase의 Query(OrderByChild)를 사용하여 서버에서 정렬
            databaseReference.Child("users").OrderByChild(stageScorePath).GetValueAsync().ContinueWithOnMainThread(task => 
            {
                // 애초부터 점수순으로 정렬된 데이터를 받아옴
                // 생략
            });
        }
        ```
        

### 3. 데이터 관리, 확장성의 효율을 위해 데이터 시스템 분리

- 구현 이유
    
    무기 데미지, 스코어 점수 등 벨런스에 관여하는 데이터은 잦은 수정이 필요한데 코드에 수치가 있을 경우 매번 코드를 수정해야 하는 비효율적인 작업을 거처야하며, 새로운 무기, 새로운 스테이지가 추가될 시에도 코드가 추가되여 가독성을 해치고 코드가 비대해질 염려가 있습니다.
    
    무엇보다도 데이터는 관리 영역이 다른 부분이라 생각하여 좀 더 쉽고 편하게 관리하고 추가 할수 있어야 한다고 생각하여 엑셀의 CSV나, 유니티의 데이터 에셋인 ScriptableObject로 따로 설계하였습니다.
    
- 상세
    1. 스테이지 보너스 점수 관리는 한눈에
        - 작은 볼륨의 많은 스테이지를 생각하고 만든 게임에 맞게 클리어시 타임 보너스를 추가하고 계산하는 기준이 되는 데이터는 한번에 많은 양을 관리 할 수 있는 익숙한 엑셀CSV를 사용 하게 설계하였고, 프로그램 동작시 효율을 챙기기 위해 데이터 파싱시 Dictionary로 캐싱하게 하여 효율을 높였습니다.
        - 관련 코드 스니펫
            
            ```csharp
            // CSVManager.cs
            public class CSVManager : MonoBehaviour
            {
                // 런타임 효율을 위해 Dictionary로 데이터 캐싱
                Dictionary<int, StageClearTimeData> clearDataDict = new Dictionary<int, StageClearTimeData>();
            
                void LoadCSVData()
                {
                    TextAsset csvData = Resources.Load<TextAsset>("TimeData");
                    string[] lines = csvData.text.Split('\n');
            
                    for (int i = 1; i < lines.Length; i++)
                    {
                        // 생략
                        string[] data = lines[i].Split(',');
            
                        // CSV 데이터를 파싱하여 객체 생성 후 Dictionary에 저장
                        StageClearTimeData stageData = new StageClearTimeData();
                        stageData.stageIndex = int.Parse(data[0]);
            						// 데이터 파싱 생략
                        clearDataDict.Add(stageData.stageIndex, stageData);
                    }
                }
            
                public StageClearTimeData GetStageClearTimeData(int stageIndex)
                {
                    if (clearDataDict.ContainsKey(stageIndex))
                        return clearDataDict[stageIndex];
                    return null;
                }
            }
            ```
            
    2. 무기, 적개체의 데이터 관리는 확장을 고려
        - 무기와 적개체에 관한 데이터는 새로운 속성들이 추가 될 확률이 높은 데이터(새로운 스킬의 쿨타임 이라 던가)들이기 때문에 데이터 속성 추가가 용이한게 좋다고 생각했습니다.
            
            그래서 ScriptableObject를 통해 데이터의 속성 추가의 편리함을 챙기고, 유니티 에디터 안에서의 데이터 조정을 편리하게 하였고, 같은 속성의 적개체를 새로운 추가 할 때에도 쉽게 새로운 ScriptObject를 할당하여 데이터를 다르게 하는 것으로 쉽게 적 개체를 늘리거나, 특별한 기믹이 없는 무기의 경우 같은 ScriptableObject로 데이터값만 다르게 하여 저격총, 기관총, 핸드건, 샷건 등 여러 무기로 확장 될 수 있었습니다.
            
        - 관련 코드 스니펫
            
            ```csharp
            // WeaponSO.cs
            [CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
            public class WeaponSO : ScriptableObject
            {
                // 데이터 수정 시 코드 변경 없이 에셋에서 값만 조절
                public GameObject WeaponPrefab;
                public int Damage = 1;
                public float FireRate = 0.1f;
                public bool isAutomatic = false;
                public int MagazineSize = 30;
            
                public float DefaultSpread = 0.1f;
                public float IncreaseSpreadPerShot = 0.01f;
                public float MaxSpread = 0.2f;
                public float DefaultRecoil = 1.0f;
                public float MaxRecoil = 5f;
            }
            
            // ActiveWeapon.cs
            public void SwitchWeapon(WeaponSO weaponSO)
            {
                // 기존 무기 제거 및 새 무기 프리팹 생성 생략
                // 무기 데이터를 교체
                this.weaponSO = weaponSO; 
                
                // 데이터에 정의된 탄창 크기 등을 반영
                currentAmmo = 0;
                keepFireRecoilPenalty = 0;
                AdjustAmmo(weaponSO.MagazineSize);
                
                // 줌 기능 여부나 감도 조절도 SO 데이터 기반으로 자동 처리
                if (!weaponSO.CanZoom) 
                {
                    // 줌 해제 로직
                }
            }
            ```
            
    

### 4. 전투 시스템

- 구현 내용
    
    기본적인 FPS게임의 타격감과 조작감을 위하여 반동, 탄퍼짐 이에 따른 크로스헤어의 벌어짐과 히트마커 연출을 구현하였고, 사격시의 충격을 연출하기 위해 시네머신 기능을 활용하여 카메라가 흔들리는 연출을 가미하였습니다.
    
- 상세
    1. 반동과 탄퍼짐 구현
        - 이를 위해 사격지속 시간과, 플레이어의 이동속도에 비례하여 탄퍼짐이 증가하고, 사격지속 시간에 비례하여 카메라가 위로 들리는 반동 시스템을 구현하였습니다.
        - **관련 코드 스니펫**
            
            ```csharp
            // FirstPersonController.cs를 분석하여 내부에 작성한 게터와 세터 함수
            
            public float GetCurrentSpeed() // 탄 퍼짐 계산을 위한 이동속도
            {
            	return new Vector3(_controller.velocity.x, 0, _controller.velocity.z).magnitude;
            }
            // 무기 반동 발생시에 카메라 피치 변경 적용
            public void ApplyRecoil(float recoilAmount)
            {
            	_cinemachineTargetPitch -= recoilAmount;
            }
            
            public float GetCurrentSpread()
            {
                float currentSpread = weaponSO.DefaultSpread;
                
                // 플레이어의 이동에 따라 탄 퍼짐 증가 
                float currentSpeed = firstPersonController.GetCurrentSpeed();
                if (currentSpeed > 0.1f)
                {
                    currentSpread += weaponSO.MoveSpreadFactor * 
                    (currentSpeed / firstPersonController.SprintSpeed);
                }
                // 연사 지속시간에 따라 탄퍼짐 증가(제곱)
                float keepFirePenalty = Mathf.Pow(keepFireRecoilPenalty, 2) * 
            												    weaponSO.IncreaseSpreadPerShot;
            												    currentSpread += keepFirePenalty;
            
                return Mathf.Min(currentSpread, weaponSO.MaxSpread);
            }
            
            void HandleShoot()
            {
                currentWeapon.Shoot(weaponSO, GetCurrentSpread());
            		//연사시간에 비례하여 카메라 반동 계산
                if (!isZoom)
                {
                    float currentRecoil = Mathf.Min(
            						       weaponSO.DefaultRecoil +
            					        (Mathf.Pow(keepFireRecoilPenalty, 2) 
            					        * weaponSO.RecoilFactor), weaponSO.MaxRecoil
            					        );
                    // firstPerconController 에 반동 적용
                    firstPersonController.ApplyRecoil(
                    currentRecoil * Time.deltaTime * 50f
                    );
                }
            		// 샷에 따른 반동 패널티 스택
                keepFireRecoilPenalty += 1f;
            }
            ```
            
    2. 부위별 피격 판정
        - 적에게 적중 시 IDamageable 인터페이스와 별도로 WeakPoint 컴포넌트를 감지하도록 구현했습니다. (약점 타격 시 전용 VFX와 추가 점수를 부여)
        - **관련 코드 스니펫**
            
            ```csharp
            // Weapon.cs
            
            // 피격 판정 및 데미지 분기
            public void HandleShootHit(RaycastHit hit, WeaponSO weaponSO)
            {
                // 기본 피격 이펙트 및 대상 설정
                GameObject vfxPrefab = weaponSO.HitVFXPrefab.gameObject;
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                bool isWeak = false;
            
                // 약점 컴포넌트(WeakPoint) 감지 시도
                if (hit.collider.TryGetComponent<WeakPoint>(out WeakPoint weakPoint))
                {
                    // 약점 전용 이펙트로 교체
                    vfxPrefab = weaponSO.CriVFXPrefab.gameObject;
                    damageable = weakPoint; 
                    isWeak = true;
                }
            
                // 이펙트 생성 데미지 전달
                PoolManager.Instance.Get(vfxPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            
                if (damageable != null)
                {
                    damageable.TakeDamage(weaponSO.Damage, hit.point, DamageType.Normal);
                    
                    // 피격 위치에 따라 크로스 해어에 히트 마크 표시(약점, 일반)
                    HitIndicator.Instance.ShowMaker(isWeak);
                }
            }
            // WeakPoint.cs 
            // 약점 타격 시 데미지 증폭
            public class WeakPoint : MonoBehaviour, IDamageable
            {
                [SerializeField] float damageMultiplier = 2.0f; // 약점 배율
            
                public void TakeDamage(int damage, Vector3 hitPoint, DamageType type)
                {
                    // 배율을 적용하여 크리티컬 데미지 연산
                    int criticalDamage = Mathf.RoundToInt(damage * damageMultiplier);
                    
                    // EnemyHealth에 크리티컬 데미지와 약점 피격 전달
                    enemyHealth.TakeDamageProcess(criticalDamage, isWeakPoint: true);
                }
            }
            ```
            
    3. 타격감 및 피격 경직 시스템
        - 적 피격 시 코루틴을 통해 이동 속도를 일시적으로 감소시켜 물리적인 타격감을 표현하였습니다.
        - **관련 코드 스니펫**
            
            ```csharp
            // Enemy.cs
            // 피격 시 적의 이동 속도를 일시적으로 감소시켜 경직
            
            protected virtual void OnDamage()
            {
                // 중복 실행 방지 기존 루틴 초기화
                if (slowRoutine != null)
                {
                    StopCoroutine(slowRoutine);
                }
            
                // 감속 코루틴 실행
                slowRoutine = StartCoroutine(OnSlowRoutine());
            }
            
            IEnumerator OnSlowRoutine()
            {
                // 이동 속도를 hitSlowFactor만큼 즉시 감소
                OnSpeedChange(hitSlowFactor);
            
                // hisSlowDuration 감속 상태 유지
                yield return new WaitForSeconds(hisSlowDuration);
            
                // 복구
                OnSpeedChange(1.0f);
                slowRoutine = null;
            }
            ```
            
    

### 5. 최적화를 위한 설계

- 구현 내용
    
    최대한 GC가 발생 하지 않도록 유의하며 코드를 설계하였습니다.
    
- 상세
    1. 물리 연산 최적화
        - 처음에 사용했던 OverlapSphere은 쓰면 매번 새로운 배열을 만들어 메모리(GC)가 쌓이는 문제가 있어, 이를 해결하기 위해 OverlapSphereNonAlloc을 사용하여 배열을 한번만 할당하고 재사용하도록 바꿨습니다.
        - 관련 코드 스니펫
            
            ```csharp
            // EnemyTactic.cs
            // 반복적인 새로운 메모리 할당을 막기 위해 NonAlloc 사용
            
            // 결과 값을 담을 배열 선언(재사용 됨)
            private Collider[] coverColliders = new Collider[10]; 
            
            public Vector3 FindCover(Transform playerPosition, Covering action)
            {
                // 매번 배열을 재생성 하지 않고 미리 만든 배열에 결과만 덮어씌움
                // 반환값은 실제로 찾은 엄페물 개수
                int cnt = Physics.OverlapSphereNonAlloc(transform.position, searchRadius, coverColliders, coverLayer);
            
                if (cnt == 0) return Vector3.zero;
            }
            ```
            
    2. 오브젝트 풀링
        - 총알이나 적처럼 자주 생기고 사라지는 오브젝트는 매번 생성/파괴하면 리소스 효율이 나쁩니다. 이를 위해 사용한 오브젝트를 끄고 대기시켰다가 다시 사용하는 풀링 시스템을 만들었습니다.
        - 관련 코드 스니펫
            
            ```csharp
            // PoolManager.cs
            // 오브젝트 생성과 파괴 부하를 줄이는 재사용 시스템
            
            public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
            {
                int id = prefab.GetInstanceID();
                
                // 대기 중인 오브젝트가 있으면 꺼내서 재사용 Dequeue
                if (poolDict.ContainsKey(id) && poolDict[id].Count > 0)
                {
                    GameObject obj = poolDict[id].Dequeue();
                    obj.SetActive(true);
                    // 위치/회전 초기화
                    return obj;
                }
                
                // 없으면 새로 생성
                else
                {
                    return Instantiate(prefab, position, rotation);
                }
            }
            ```
            
    3. 성능 측정 툴 제작
        - 최적화를 실질적인 데이터로 확인하기 위해 프레임과 메모리 사용량, GC메모리와 카운터를 텍스트 CSV로 저장하는 기능을 추가하여 최적화 전후 성능 차이를 숫자로 비교할 수 있게되었습니다.
        - 관련 코드 스니펫
            
            ```csharp
            // OptimizationMeasurement.cs
            // 게임 실행 중 FPS와 메모리, GC 사용량을 CSV로 기록
            
            void OnEnable()
            {
                // GC 할당량을 추적하는 레코더 시작
                profilerRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            }
            
            void RecordData()
            {
                // 현재 프레임의 FPS, 메모리, GC를 수치로 변환
                float fps = 1.0f / deltaTime;
                float gcAlloc = profilerRecorder.LastValue / 1024f; // KB 단위
                
                // CSV 형식으로 저장하기 위해 텍스트 기록
                csvContent.AppendLine($"{Time.time:F1},{fps:F1},{totalMem:F1},{gcAlloc:F1}");
            }
            ```
            
        

---

## 4. 문제 해결 및 기술적 도전

1. **firebase 연동 로그인 문제 (비동기)**
    - 발생이슈
        
        로그인 기능을 구현했으나, 앱 실행 초기 파이어베이스의 초기화(비동기)가 완료되기 전에 메인 메뉴 UI가 먼저 로드되는 문제가 발생했습니다. 
        
        이로 인해 이미 로그인된 유저임에도 불구하고, 로그인 버튼이 노출되거나 닉네임이 UnknownPlayer(플플레이어 데이터를 찾지못하면 나오는 이름)로 나오는 레이스 컨디션 이슈가 있었습니다.
        
    - 원인
        
        유니티의 생명주기와 파이어베이스의 네크워크 데이터 연동 시점이 동기화 되지 않아 데이터를 받아 오지 못한 상태에서 UI 갱신 로직이 실행되어서 발생한 문제 였습니다.
        
    - 해결 방법
        
        초기화 상태를 확인하기위해 AuthManager에 IsFirebaseReady 라는 플레기를 선언하고, 초기화상태에 따라 상태를 받을 수  있게 하였습니다.
        
        그리고 MainMenuHandler에 AuthManger의 Ready플레그를 기다리는 코루틴을 작성하여 AuthManager가 초기화완료 시점에 UI를 연결하도록 대기 하는 코루틴을 추가하였습니다.
        
    - 관련 코드 스니펫
        
        ```csharp
        // MainMenuHandler.cs
        
        // 파이어베이스 인증 초기화 대기 루틴
        IEnumerator AuthInitWaitRoutine()
        {
            // 일단 버튼들을 비활성화 연결이 되지 않으면 아예 클릭도 못하도록
            loginButton.SetActive(false);
            logoutButton.SetActive(false);
        
            // 인증 매니저가 생성될 때까지 대기
            while (AuthManager.Instance == null)
            {
                yield return null;
            }
        
            // 이벤트 연결
            AuthManager.Instance.OnLoginSuccess += RefreshAuthUI;
            AuthManager.Instance.OnLogout += OnLogout;
        
            // 파이어베이스가 완전히 준비될 때까지 대기
            while (!AuthManager.Instance.IsFirebaseReady)
            {
                yield return null;
            }
        
            // 준비 완료 후에 유저 상태에 맞춰 갱신
            RefreshAuthUI(AuthManager.Instance.CurrentUser);
        }
        
        // AuthManager.cs
        void InitializeAuth()
        {
            // 비동기 의존성 확인(CheckAndFixDependenciesAsync)
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    firebaseAuth = FirebaseAuth.DefaultInstance;
                    
                    // 초기화 생략
        
                    // 초기화가 끝났음을 알리는 플래그 설정
                    IsFirebaseReady = true; 
                }
            });
        }
        ```
        
    
2. **firebase 특성으로 인한 스테이지 별 데이터 파싱 오류 발생**
    - 발생이슈
        
        스테이지별 점수 데이터를 연동하는 과정에서, 종합 랭킹은 정상적으로 표시되나, 스테이지별 랭킹 조회 시 데이터가 로드되지 않고 리더보드 목록이 전부 초기값(점수 0, 이름 없음)으로만 표시되는 문제가 발생했습니다.
        
    - 원인
        
        Firebase Realtime Database는 키값이 0, 1, 2와 같이 연속된 정수 인덱스일 경우 네트워크 효율을 위해 Dictionary가 아닌 List 형태로 데이터를 반환한다는 걸 알게 되었습니다.
        기존 코드는 데이터를 Dictionary<string, object>로 캐스팅 하도록 작성되어 있었기 때문에, List 형태로 들어온 데이터를 처리하지 못하고 캐스팅 실패, 아예 파싱 로직 자체가 동작하지 않았었습니다.
        
    - 해결방법
        
        Dictionary형 변환 방식을 버리고 Firebase SDK의 DataSnapshot 메서드Child(), HasChild(), Exists를 활용하는 것으로 수정하였습니다.
        이를 통해 서버가 데이터를 어떤 방식으로 반환하든, 키값을 중심으로 값에 접근할 수 있게 설계하여 해결하였습니다.
        
    - 관련 코드 스니펫
        
        ```csharp
        // FirebaseManager.cs 
        // Dictionary 캐스팅을 제거하고 Snapshot API로 직접 접근
        
        if (stageIndex == 0) { // 종합 점수 처리 로직 }
        else
        {
            // 수정전 (Dictionary<string, object>)data.Value["stages"]-> 리스트로 오기 때문에 받을 수 없음
            // 수정후: data.Child("stages").Child(index) -> 데이터 구조에 상관없이 스냅샷으로 받아옴
        
            // stages 노드와 stageIndex가 존재하는지 Snapshot 메서드로 안전하게 확인 없으면 다음 스테이지 체크
            if (!data.HasChild("stages") || !data.Child("stages").Child(stageIndex.ToString()).Exists) 
                continue;
        
            DataSnapshot stageSnap = data.Child("stages").Child(stageIndex.ToString());
            
            // 값 추출도 Child().Value로
            if (stageSnap.HasChild("score")) 
                uScore = Convert.ToInt32(stageSnap.Child("score").Value);
            
            if (stageSnap.HasChild("time")) 
                uTime = Convert.ToSingle(stageSnap.Child("time").Value);
                
            // ...
        }
        ```
        
    
3. **길고 복잡한 EnemyBrain.cs 의 전투 상태 로직을 유동적으로 추가 활용하기 위한 상태 패턴 도입**
    - 발생이슈
        
        초기 AI 로직은 enum과 switch 문을 사용하여 하나의 Update 함수 안에서 모든 분기를 처리했습니다.
        하지만 새로운 적 척탄병을 추가하고 난 뒤 코드가 너무나도 복잡하고 난잡하고 다음에 추가할 새로운 타입의 적을 추가 한다면 RangeCombatRoutine을 상속하여 또 새로운 적의 루틴을 전투루틴에 새로 작성 할 생각을 하니 이게 맞는 방법인가 진짜 이게 FSM이라고 볼수 있나 싶었습니다.
        
        증가한 복잡도는 차치하고서라도 똑같은 코드를 복사해와서 그 코드 로직안에 충돌없이 새로운 로직을 짜넣기란 생각만해도 머리가 아파왔습니다.
        
        그리하여 간단하게 상태를 정의하는 클래스들을 만들어 파츠를 갈아 끼우듯이 코드를 변경하기로하였습니다.
        
    - 시행착오
        
        처음에는 기존과 별 다를 바 없이 CombatState를 상속받은 GrenadierCombatState를 만들어 사격과 투척을 동시에 처리하려 했습니다. 
        하지만 이는 전과 모양만 다를 뿐 똑같은 구조를 가지고 있었고, 다음 추가가 아니라 지금 베이직 코드를 정해놓고 상태 별로 전환이라는 형태로 구상하였고, 이 과정에서 투척이라는 로직을 하나의 상태로 빼기로 하였습니다.
        
    - 문제 해결
    그리해여 투척을 GrenadeThrowState라는 하나의 클래스로 분리했습니다.
    전투 중 투척 조건이 만족되면 RangeCombatState에서 GrenadeThrowState로 상태가 바뀌고 투척 후 다시 SearchState로 복귀하는 순환 루프를 만들었습니다.
    이러한 결과로 새로운 행동 패턴을 추가할 때 기존 코드를 건드리지 않고 새로운 상태를 넣어주면 되도록 변경되었습니다.
    - 관련 코드 스니펫
        
        ```csharp
        // EnemyBrain.cs
        public abstract class EnemyBrain : Enemy
        {
            // 리펙토링 전 enum과 switch문으로 관리
            // public enum AIState { Patrol, Combat, Search }
            // [SerializeField] protected AIState currentState;
            
            // [리팩토링 후] 상태 패턴 적용하여 행동 자체를 객체로 관리
            public BaseEnemyState PatrolState { get; protected set; }
            public BaseEnemyState CombatState { get; protected set; }
            public BaseEnemyState SearchState { get; protected set; }
        
            protected BaseEnemyState currentState;
            
            // 생량
            
            // 거대한 Switch문 대신 현재 상태의 Execute()만 호출 해당상태의 로직이 실행
            protected override void Update()
            {
                // ...
                currentState?.Execute();
            }
        }
        
        // Grenadier.cs
        public override void OnCombatFinish()
        {
            float dist = Vector3.Distance(transform.position, LastPlayerPosition);
        
            // 투척 가능 조건만 확인 맞다면 투척 상태로 전환
            if (CanThrowGrenade(dist))
            {
                ChangeState(ThrowState); // GrenadeThrowState로 전이
            }
            else
            {
                ChangeState(SearchState); // 아니라면 다시 서치모드
            }
        }
        
        // GrenadeThrowState.cs
        public class GrenadeThrowState : BaseEnemyState
        {
            // 생략
            public override void Enter()
            {
                grenadier.Agent.isStopped = true;
                grenadier.Animator.SetTrigger("ThrowGrenade");
                // 생략
                grenadier.StartStateCoroutine(ThrowGrenadeRoutine());
            }
        
            IEnumerator ThrowGrenadeRoutine()
            {
                yield return new WaitForSeconds(1.0f);
                // 투척 행동이 끝나면 해당하는 다음 상태로 넘김
                grenadier.ChangeState(brain.SearchState);
            }
        }
        ```
        
    
4. **초기화 순서, 동적 객체 Player 참조 문제 해결**
    - 발생 이슈
    싱글톤 매니저들이 초기화되기 전에 다른 스크립트가 매니저를 호출하여 NullReferenceException이 간헐적으로 발생하는 이슈가 있었고,  GameInitializer를 작성하여 매니저들의 생성 순서를 명시적으로 정하였으나 인게임 씬 로드 직후 적이나 UI가 OnEnable에서 플레이어를 참조하려다 실패하는 문제가 발생하였습니다.
    - 해결 과정
        
        실행 순서 강제 
        우선 유니티의 Script Execution Order를 조정하여 플레이어 관련 스크립트들을 먼저 실행시켰서 해결했으나 이는 프로젝트가 커질수록 관리 포인트가 초기화 문제 발생시 원인을 찾기 힘든 구조라 생각하였습니다..
        
    - 문제 해결
    GameManager를 기준으로 플레이어가 생성되고 등록되는 시점을 기준으로 플레이어 초기화 완료 이벤트를 날리기로 하였습니다.
        1. PlayerSpawner가 의존성(카메라, UI)을 직접 주입하여 초기값을 세팅한후 플레이어 초기화를 호출하고 GameManager에 등록합니다.
        2. GameManager는 플레이어 등록 이벤트인 OnPlayerRegistered 이벤트를 날립니다.
        3. 적이나 UI는 이 이벤트를 구독하여 플레이어가 생성된 시점에 안전하게 참조를 가져오도록 수정했습니다.
    - 관련 코드 스니펫
        
        ```csharp
        // GameInitializer.cs
        IEnumerator InitializeGame()
        {
            // 순차적으로 매니저들을 초기화하여 의존성 문제 해결
            yield return StartCoroutine(InitManager(csvManagerPrefab, "CSV Manager"));
            yield return StartCoroutine(InitManager(soundManagerPrefab, "Sound Manager"));
            yield return StartCoroutine(InitManager(poolManagerPrefab, "Pool Manager"));
            
            // 생략
            
            // 모든 매니저 준비 완료 후 메인 메뉴 진입
            SceneManager.LoadScene(Constants.SCENE_MAIN_MENU);
        }
        
        // PlayerSpawner.cs
        public void SpawnPlayer()
        {
            // 플레이어 생성
            GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
            
            // 생성 직후 의존성 주입
            InjectDependencies(playerInstance);
        }
        
        void InjectDependencies(GameObject playerObject)
        {
            // 초기화 세팅 생략
            
            // GameManager에 플레이어 등록 요청(이벤트 발생)
            GameManager.Instance.FindPlayer(playerHealth, cameraRoot);
        }
        
        // GameManager.cs
        public event Action<PlayerHealth, Transform> OnPlayerRegistered;
        
        public void FindPlayer(PlayerHealth playerHealth, Transform targetPoint)
        {
            Player = playerHealth;
            // 생략
            // 플레이어가 등록됨을 구독 클래스에 알림
            OnPlayerRegistered?.Invoke(Player, targetPoint);
        }
        
        // [Enemy.cs]
        protected virtual void OnEnable()
        {
            // 생략
            
            // 
            // 플레이어가 있으면 인스턴스 참조하여 바로 초기화
            if (GameManager.Instance.Player != null)
            {
                InitializePlayer(GameManager.Instance.Player, GameManager.Instance.PlayerTargetPoint);
            }
            
            // 아직 플레이어가 생성 전이라면 등록 이벤트를 구독해서 대기
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerRegistered += InitializePlayer;
            }
        }
        ```
        
    
5. **체감하기 어려운 최적화 문제**
- 이슈
    
    이번프로젝트는 최적화 전과 후 딱히 크게 달라진점이 체감되지 않는 경우가 많았습니다.
    
- 해결 방법
    
    FPS(프레임), 메모리 사용량, GC(가비지 컬렉터) 발생 횟수를 실시간으로 측정하여 CSV 파일로 저장하는 툴(OptimizationMeasurement)을 작성해 보았습니다.
    
    - 관련 코드 스니펫
        
        ```csharp
        // OptimizationMeasurement.cs
        void Update()
        {
            if (m_timeCounter < m_refreshTime)
            {
                m_timeCounter += Time.deltaTime;
                m_frameCounter++;
            }
            else
            {
                // GC 횟수, 메모리 사용량 수집
                int gcCount = GC.CollectionCount(0); 
                long memory = GC.GetTotalMemory(false);
                
                // StringBuilder로 CSV 포맷 데이터 생성 (GC 최소화 노력)
                m_sb.Append($"{System.DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss")},{m_lastFramerate},{memory},{gcCount}");
                
                // 생략
            }
        }
        ```
        

![스크린샷 2025-12-18 11.59.31.png](FPS%20RobotShooter/%E1%84%89%E1%85%B3%E1%84%8F%E1%85%B3%E1%84%85%E1%85%B5%E1%86%AB%E1%84%89%E1%85%A3%E1%86%BA_2025-12-18_11.59.31.png)

![스크린샷 2025-12-18 11.59.45.png](FPS%20RobotShooter/%E1%84%89%E1%85%B3%E1%84%8F%E1%85%B3%E1%84%85%E1%85%B5%E1%86%AB%E1%84%89%E1%85%A3%E1%86%BA_2025-12-18_11.59.45.png)

---

## 5. 개발 과정, 교훈
