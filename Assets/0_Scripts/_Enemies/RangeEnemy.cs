using UnityEngine;
using System.Collections;
using Unity.Mathematics;

public class RangeEnemy : EnemyBrain
{
    public enum Strategy
    {
        Equal,
        DisAdvFar,
        DisAdvNear,
        AdvFar,
        AdvNear
    }

    [SerializeField] EnemyBodySO enemyBodySO;
    [SerializeField] float combatDistance = 15f;
    [SerializeField] ParticleSystem deathParticle;

    EnemyWeaponController enemyWeaponController;
    PlayerHealth playerHealth;

    float lastAttacTime;
    bool firstAttack = false;

    protected override void Awake()
    {
        base.Awake();
        enemyWeaponController = GetComponent<EnemyWeaponController>();

        sight.viewDistance = enemyBodySO.DetectionRadius;
        sight.viewAngle = enemyBodySO.ViewAngle;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        agent.speed = enemyBodySO.MoveSpeed;
        enemyHealth.InitializeHealth(enemyBodySO.MaxHP);
        Debug.LogWarning($"{enemyHealth.CurrentHP}");
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        firstAttack = false;
        enemyHealth.OnDeath += Death;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        enemyHealth.OnDeath -= Death;
    }

    protected override void OnFound()
    {
        base.OnFound();
        firstAttack = true;
    }

    Strategy DetermineStrategy()
    {
        if (playerHealth == null) return Strategy.Equal;

        int myHP = enemyHealth.CurrentHP;
        int playerHP = playerHealth.CurrentHP;
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (Mathf.Abs(myHP - playerHP) <= 40)
        {
            Debug.Log("동등");
            return Strategy.Equal;
        }
        else if (myHP < playerHP)
        {
            if (dist > combatDistance)
            {
                Debug.Log("약세 멈");
                return Strategy.DisAdvFar;
            }
            else
            {
                Debug.Log("약세 가까움");
                return Strategy.DisAdvNear;
            }
        }
        else
        {
            if (dist > combatDistance)
            {
                Debug.Log("우세 멈");
                return Strategy.AdvFar;
            }
            else
            {
                Debug.Log("우세 가까움");
                return Strategy.AdvNear;
            }
        }
    }

    protected override IEnumerator CombatRoutine()
    {
        isActing = true;

        if (firstAttack)
        {
            Debug.Log("기습공격");
            agent.isStopped = true;

            if (playerTransform != null)
            {
                Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            yield return StartCoroutine(enemyWeaponController.FireBurst(playerTransform.position));

            firstAttack = false;
        }

        Strategy strategy = DetermineStrategy();
        agent.isStopped = false;
        agent.speed = enemyBodySO.MoveSpeed;
        agent.updateRotation = false;
        bool canShoot = true;
        Vector3 coveringPosition = transform.position;

        switch (strategy)
        {
            case Strategy.Equal:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.Near);
                break;

            case Strategy.DisAdvFar:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.FarPlayer);
                canShoot = false;
                agent.updateRotation = true;
                agent.speed = enemyBodySO.MoveSpeed * 1.5f;
                break;

            case Strategy.DisAdvNear:
                coveringPosition = transform.position;
                agent.isStopped = true;
                break;

            case Strategy.AdvFar:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.NearPlayer);
                break;

            case Strategy.AdvNear:
                coveringPosition = playerTransform.position;
                break;
        }

        if (!agent.isStopped && coveringPosition != Vector3.zero)
        {
            agent.SetDestination(coveringPosition);
        }

        float actionTimer = 0f;
        while (actionTimer < 2.0f)
        {
            actionTimer += Time.deltaTime;

            if (playerTransform != null && strategy != Strategy.DisAdvFar)
            {
                Vector3 lookTarget = sight.CanSeePlayer(playerTransform) ? playerTransform.position : lastPlayerPosition;
                Vector3 lookDir = (lookTarget - transform.position).normalized;
                lookDir.y = 0;

                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(lookDir);
                }
            }

            if (canShoot && Time.time >= lastAttacTime + enemyWeaponController.FireRate)
            {
                if (strategy == Strategy.AdvNear || strategy == Strategy.DisAdvNear || sight.CanSeePlayer(playerTransform))
                {
                    Vector3 fireTarget = playerTransform.position;
                    StartCoroutine(enemyWeaponController.FireBurst(fireTarget));
                    lastAttacTime = Time.time;
                }
            }

            if (strategy == Strategy.AdvNear)
            {
                agent.SetDestination(playerTransform.position);
            }
            yield return null;
        }
        agent.updateRotation = true;
        isActing = false;
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyBodySO.DetectionRadius);
        Vector3 eyePos = sight.EyePosition;
        Vector3 leftView = Quaternion.Euler(0, -enemyBodySO.ViewAngle / 2, 0) * sight.EyeTransform.forward;
        Vector3 rightView = Quaternion.Euler(0, enemyBodySO.ViewAngle / 2, 0) * sight.EyeTransform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(eyePos, leftView * enemyBodySO.DetectionRadius);
        Gizmos.DrawRay(eyePos, rightView * enemyBodySO.DetectionRadius);

        Vector3 dirToTarget = (playerTransform.position - eyePos).normalized;

        bool isHit = Physics.Raycast(eyePos, dirToTarget, out RaycastHit hit, enemyBodySO.DetectionRadius, sight.ViewLayerMask);

        if (isHit && hit.transform.root.CompareTag("Player"))
        {
            Gizmos.color = Color.green;

        }
        else
        {
            Gizmos.color = Color.red;
        }
        Gizmos.DrawLine(eyePos, playerTransform.position);
    }

    void Death()
    {
        Instantiate(deathParticle, transform.position, quaternion.identity);
        Destroy(gameObject);
    }

    protected override void OnSpeedChange(float speedFactor)
    {
        agent.speed = enemyBodySO.MoveSpeed * speedFactor;
    }
    protected override bool IsTargetInRange(float dist) => true;
    protected override void Move() { }
    protected override void TryAttack() { }



    // public enum AIState { Patrol, Combat, Search }
    // public enum Covering { Near, FarPlayer, NearPlayer }

    // [SerializeField] EnemyWeaponSO enemyWeaponSO;
    // [SerializeField] LayerMask coverLayer;
    // [SerializeField] Transform eyePosition;
    // [SerializeField] LayerMask viewLayerMask;
    // [SerializeField] Transform firePoint;

    // [SerializeField] AIState currentState;
    // [SerializeField] Strategy currentStrategy;
    // [SerializeField] float combatTime = 5f;

    // NavMeshAgent agent;
    // Vector3 lastPlayerPosition;
    // float combatDuration;
    // bool isActing = false;
    // protected override void OnDamage()
    // {
    //     if (currentState == AIState.Combat) return;

    //     StopAllCoroutines();
    //     isActing = false;

    //     base.OnDamage();

    //     Vector3 lookDir = (playerTransform.position - transform.position).normalized;
    //     lookDir.y = 0;

    //     if (lookDir != Vector3.zero)
    //     {
    //         transform.rotation = Quaternion.LookRotation(lookDir);
    //     }

    //     if (currentState != AIState.Combat)
    //     {
    //         Debug.Log("기습당함");
    //         currentState = AIState.Combat;
    //         lastPlayerPosition = playerTransform.position;
    //         combatDuration = 0;
    //     }
    // }
    // protected override void Update()
    // {
    //     if (playerTransform == null) return;

    //     if (CanSeePlayer())
    //     {
    //         lastPlayerPosition = playerTransform.position;
    //         combatDuration = 0;
    //     }
    //     else if (currentState == AIState.Combat)
    //     {
    //         combatDuration += Time.deltaTime;
    //         if (combatDuration > combatTime)
    //         {
    //             SearchMode();
    //             return;
    //         }
    //     }

    //     switch (currentState)
    //     {
    //         case AIState.Patrol:
    //             UpdatePatrol();
    //             break;
    //         case AIState.Combat:
    //             UpdateCombat();
    //             break;
    //         case AIState.Search:
    //             UpdateSearch();
    //             break;
    //     }
    // }

    // void UpdatePatrol()
    // {
    //     Debug.LogWarning("정찰모드");

    //     if (!agent.updateRotation)
    //     {
    //         agent.updateRotation = true;
    //     }

    //     if (CanSeePlayer())
    //     {
    //         Debug.Log("정찰중 적찾음");
    //         firstAttack = true;
    //         currentState = AIState.Combat;
    //         return;
    //     }

    //     if (!agent.pathPending && agent.remainingDistance < 0.5f)
    //     {
    //         Vector3 randomPoint = GetRandomPointOnNavMesh(transform.position, 10f);
    //         agent.SetDestination(randomPoint);
    //     }

    // }

    // void UpdateCombat()
    // {
    //     Debug.LogWarning("전투모드");
    //     if (isActing) return; // 이미 행동 중이면 리턴

    //     currentStrategy = DetermineStrategy();
    //     StartCoroutine(StrategyActionRoutine(currentStrategy));
    // }

    // void UpdateSearch()
    // {
    //     Debug.LogWarning("서치모드");

    //     if (CanSeePlayer())
    //     {
    //         Debug.Log("수색중 적 찾음");
    //         firstAttack = true;
    //         currentState = AIState.Combat;
    //         StopAllCoroutines();
    //         isActing = false;
    //         return;
    //     }

    //     if (!isActing)
    //     {
    //         StartCoroutine(SearchRoutine());
    //     }

    // Vector3 lookDir = (lastPlayerPosition - transform.position).normalized;
    // lookDir.y = 0;

    // if (lookDir != Vector3.zero)
    // {
    //     transform.rotation = Quaternion.LookRotation(lookDir);
    // }

    // agent.SetDestination(lastPlayerPosition);

    // // 도착 후 
    // if (Vector3.Distance(transform.position, lastPlayerPosition) < 2f)
    // {
    //     currentState = AIState.Patrol;
    // }
    // }

    // IEnumerator SearchRoutine()
    // {
    //     isActing = true;
    //     agent.isStopped = false;
    //     agent.speed = enemyBodySO.MoveSpeed;
    //     agent.updateRotation = false;

    //     int wayPointCounter = Random.Range(1, 3);

    //     for (int i = 0; i < wayPointCounter; i++)
    //     {
    //         Vector3 bluffPosition = tactic.GetRandomWayPoint(lastPlayerPosition, 10f);

    //         agent.SetDestination(bluffPosition);

    //         while (agent.pathPending || agent.remainingDistance > .5f)
    //         {
    //             if (CanSeePlayer())
    //             {
    //                 isActing = false;
    //                 yield break;
    //             }
    //             Vector3 lookDir = (lastPlayerPosition - transform.position).normalized;
    //             lookDir.y = 0;
    //             if (lookDir != Vector3.zero)
    //             {
    //                 transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
    //             }
    //             yield return null;
    //         }
    //     }

    //     agent.SetDestination(lastPlayerPosition);

    //     while (agent.pathPending || agent.remainingDistance > 0.5f)
    //     {
    //         if (CanSeePlayer())
    //         {
    //             isActing = false;
    //             yield break;
    //         }
    //         yield return null;
    //     }

    //     Debug.Log("수색 모드에서 정찰 모드로 전환");
    //     currentState = AIState.Patrol;
    //     isActing = false;
    // }

    // IEnumerator FireBurst()
    // {
    //     for (int i = 0; i < enemyWeaponSO.BurstCount; i++)
    //     {
    //         if (playerTransform == null) break;
    //         FireProjectile();
    //         yield return new WaitForSeconds(0.1f);
    //     }
    // }

    // void FireProjectile(Transform playerPosition)
    // {
    //     Vector3 spawnPosition = firePoint.position;
    //     Vector3 targetPosition = playerPosition.position;
    //     Vector3 direction = (targetPosition - spawnPosition).normalized;

    //     float accErr = enemyWeaponSO.AccuracyError;

    //     direction.x += Random.Range(-accErr, accErr) * .01f;
    //     direction.y += Random.Range(-accErr, accErr) * .01f;
    //     direction.z += Random.Range(-accErr, accErr) * .01f;

    //     GameObject newProjectile = PoolManager.Instance.Get(enemyWeaponSO.ProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
    //     if (newProjectile.TryGetComponent<Projectile>(out Projectile p))
    //     {
    //         p.Initialize(enemyWeaponSO.Damage, enemyWeaponSO.ProjectileSpeed, enemyWeaponSO.ProjectileLifeTime);
    //     }
    //     Debug.Log("적발사");
    // }

    // Vector3 GetRandomPointOnNavMesh(Vector3 center, float range)
    // {
    //     Vector3 randomPosition = center + UnityEngine.Random.insideUnitSphere * range;
    //     NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, range, NavMesh.AllAreas);
    //     return hit.position;
    // }

    // bool CanSeePlayer()
    // {
    //     if (playerTransform == null) return false;
    //     Vector3 eyePos = eyePosition.position;
    //     Vector3 targetDir = (playerTransform.position - eyePos).normalized;
    //     float angle = Vector3.Angle(eyePosition.forward, targetDir);

    //     RaycastHit hit;
    //     if (angle < enemyBodySO.ViewAngle / 2f)
    //     {
    //         if (Physics.Raycast(eyePos, targetDir, out hit, enemyBodySO.DetectionRadius, viewLayerMask))
    //         {
    //             if (hit.transform.CompareTag(Constants.PLAYER_TAG)) return true;
    //         }
    //     }
    //     return false;
    // }

    // Vector3 FindCover(Covering action)
    // {
    //     Debug.Log("장애물 찾기");
    //     Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, coverLayer);
    //     if (colliders.Length == 0) return Vector3.zero;

    //     List<Collider> bestCover = null;

    //     switch (action)
    //     {
    //         case Covering.Near:
    //             // 나랑 제일 가까운 거
    //             bestCover = colliders.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();
    //             Debug.Log("장애물 찾기 액션 체크중 그냥 가장 가까운 장애물");
    //             break;

    //         case Covering.FarPlayer:
    //             bestCover = colliders.OrderByDescending(c => Vector3.Distance(playerTransform.position, c.transform.position)).ToList();
    //             Debug.Log("장애물 찾기 액션 체크중플레이어로부터멈");
    //             break;

    //         case Covering.NearPlayer:
    //             bestCover = colliders.OrderBy(c => Vector3.Distance(playerTransform.position, c.transform.position)).ToList();
    //             Debug.Log("장애물 찾기 액션 체크중 플레이어에게가까움");
    //             break;
    //     }

    //     foreach (var c in bestCover)
    //     {
    //         Debug.Log("장애물 체크");
    //         Vector3 hideDir = (c.transform.position - playerTransform.position).normalized;
    //         Vector3 hidePos = c.transform.position + hideDir * 2.0f;

    //         if (NavMesh.SamplePosition(hidePos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
    //         {
    //             return hit.position;
    //         }
    //     }
    //     Debug.Log("못찾음");
    //     return transform.position;
    // }

    // void SearchMode()
    // {
    //     currentState = AIState.Search;
    //     agent.ResetPath();
    //     isActing = false;
    // }
}


