using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class RangeEnemy : Enemy
{
    public enum AIState { Patrol, Chase, Combat, Search }
    public enum Strategy { Aggressive, Defensive };

    [SerializeField] EnemyBodySO enemyBodySO;
    [SerializeField] EnemyWeaponSO enemyWeaponSO;
    [SerializeField] LayerMask coverLayer;
    [SerializeField] Transform eyePosition;
    [SerializeField] LayerMask viewLayerMask;

    [SerializeField] AIState currentState;
    [SerializeField] Strategy currentStrategy;

    NavMeshAgent agent;
    Vector3 lastTargetPos;
    float lastAttacTime;
    float searchTimer;
    bool isActing = false;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemyBodySO.MoveSpeed;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        currentState = AIState.Patrol;
        isActing = false;
        searchTimer = 0;
    }

    protected override void Update()
    {
        if (playerTarget == null) return;

        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrol();
                break;
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.Combat:
                UpdateCombat();
                break;
            case AIState.Search:
                UpdateSearch();
                break;
        }
    }

    void UpdatePatrol()
    {
        if (CanSeeTarget())
        {
            Debug.Log("원거리 적: 찾음");
            currentState = AIState.Chase;
            return;
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, 10f);
            agent.SetDestination(randomPoint);
        }
        // TODO: 순찰로직
    }

    void UpdateChase()
    {
        Debug.Log("원거리 적: 추노 ㄱㄱ");
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        // 사거리 안에 들어왔고 시야에 들어와 있으면 전투 모드
        if (dist <= enemyWeaponSO.AttackRange && CanSeeTarget())
        {
            Debug.Log("원거리 적: 전투모드");
            currentState = AIState.Combat;
            agent.ResetPath();
            return;
        }

        // 시야에서 놓쳤다면 마지막 위치로 서치
        if (!CanSeeTarget())
        {
            Debug.Log("원거리 적: 시야에서 놓침");
            lastTargetPos = playerTarget.position;
            currentState = AIState.Search;
            return;
        }

        // 계속 추격
        agent.SetDestination(playerTarget.position);
    }

    void UpdateCombat()
    {
        // 찾지못한 상태라면 리턴
        if (!CanSeeTarget())
        {
            currentState = AIState.Search;
            lastTargetPos = playerTarget.position;
            return;
        }

        if (isActing) return; // 이미 사격,엄폐 행동 중이면 리턴

        // 전략판단 로직
        Debug.Log("원거리 적: 전투모드 들어음 전략 판단 ㄱㄱ");
        currentStrategy = DetermineAction();

        if (currentStrategy == Strategy.Defensive)
        {
            StartCoroutine(CoverShootRoutine());
        }
        else
        {
            StartCoroutine(AggressiveShootRoutine());
        }
    }

    void UpdateSearch()
    {
        if (CanSeeTarget())
        {
            Debug.Log("수색중 적 찾음");
            currentState = AIState.Combat;
            return;
        }
        Debug.Log("수색 ㄱㄱ");
        // 마지막 목격 지점으로 이동
        agent.SetDestination(lastTargetPos);

        // 도착 후 
        if (Vector3.Distance(transform.position, lastTargetPos) < 2f)
        {
            Debug.Log("목격 지점 도착");
            searchTimer += Time.deltaTime;
            // 일정 시간 지나면 다시 정찰
            if (searchTimer > enemyBodySO.LostTargetSearchTime)
            {
                Debug.Log("없네...");
                currentState = AIState.Patrol;
                searchTimer = 0;
            }
        }
    }

    Strategy DetermineAction()
    {
        float aggressiveScore = 0f;
        float defensiveScore = 0f;

        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist > 10f)
        {
            defensiveScore += 10f;
        }
        else
        {
            aggressiveScore += 10f;
        }

        if (!HasCoverNearBy()) defensiveScore -= 100f;

        Vector3 playerDirTo = (transform.position - playerTarget.position).normalized;

        if (Vector3.Dot(playerTarget.forward, playerDirTo) > 0.8f)
        {
            defensiveScore += 30f;
        }
        else
        {
            aggressiveScore += 10f;
        }
        Debug.Log($"원거리 적:{defensiveScore}:{aggressiveScore}");
        return (defensiveScore > aggressiveScore) ? Strategy.Defensive : Strategy.Aggressive;
    }

    IEnumerator CoverShootRoutine()
    {
        isActing = true;

        Vector3 coverPos = FindCoverPos();
        if (coverPos != Vector3.zero)
        {
            agent.SetDestination(coverPos);

            yield return new WaitForSeconds(1f);
        }

        Vector3 peekPos = transform.position + transform.right * 1f;
        agent.SetDestination(peekPos);

        yield return new WaitForSeconds(.5f);

        yield return StartCoroutine(FireBurst());

        agent.SetDestination(coverPos);

        yield return new WaitForSeconds(1f);

        isActing = false;
    }

    IEnumerator AggressiveShootRoutine()
    {
        isActing = true;

        agent.isStopped = true;
        transform.LookAt(playerTarget);

        yield return StartCoroutine(FireBurst());

        agent.isStopped = false;
        yield return new WaitForSeconds(enemyWeaponSO.FireRate);

        isActing = false;
    }

    IEnumerator FireBurst()
    {
        for (int i = 0; i < enemyWeaponSO.BurstCount; i++)
        {
            if (playerTarget == null) break;


            FireProjectile();
            yield return new WaitForSeconds(0.1f);
        }
    }

    void FireProjectile()
    {
        // TODO: 정확도(AccuracyError) 적용 해서 총알 발사

        Debug.Log("적군 발사!");
    }

    Vector3 GetRandomPointOnNavMesh(Vector3 center, float range)
    {
        Vector3 randomPosition = center + UnityEngine.Random.insideUnitSphere * range;
        NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, range, NavMesh.AllAreas);
        return hit.position;
    }

    bool CanSeeTarget()
    {
        if (playerTarget == null) return false;
        Vector3 eyePos = eyePosition.position;
        Vector3 targetDir = (playerTarget.position - eyePos).normalized;
        float angle = Vector3.Angle(eyePosition.forward, targetDir);

        RaycastHit hit;
        if (angle < enemyBodySO.ViewAngle / 2f)
        {
            if (Physics.Raycast(eyePos, targetDir, out hit, enemyBodySO.DetectionRadius, viewLayerMask))
            {
                if (hit.transform.CompareTag(Constants.PLAYER_TAG)) return true;
            }
        }
        return false;
    }
    void OnDrawGizmos()
    {
        if (playerTarget == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyBodySO.DetectionRadius);
        Vector3 eyePos = eyePosition.position;
        Vector3 leftView = Quaternion.Euler(0, -enemyBodySO.ViewAngle / 2, 0) * eyePosition.forward;
        Vector3 rightView = Quaternion.Euler(0, enemyBodySO.ViewAngle / 2, 0) * eyePosition.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(eyePos, leftView * enemyBodySO.DetectionRadius);
        Gizmos.DrawRay(eyePos, rightView * enemyBodySO.DetectionRadius);

        Vector3 dirToTarget = (playerTarget.position - eyePos).normalized;

        bool isHit = Physics.Raycast(eyePos, dirToTarget, out RaycastHit hit, enemyBodySO.DetectionRadius, viewLayerMask);

        if (isHit && hit.transform.root.CompareTag("Player"))
        {
            Gizmos.color = Color.green;

        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawLine(eyePos, playerTarget.position);
    }

    bool HasCoverNearBy()
    {
        Debug.Log("엄페물 찾기");
        return Physics.CheckSphere(transform.position, 5f, coverLayer);
    }

    Vector3 FindCoverPos()
    {
        return transform.position;
    }

    protected override void OnSpeedChange(float speedFactor)
    {
        agent.speed = enemyBodySO.MoveSpeed * speedFactor;
    }
    protected override bool IsTargetInRange(float dist) => true;
    protected override void Move() { }
    protected override void TryAttack() { }
}
