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
    [SerializeField] float combatDistance;
    [SerializeField] ParticleSystem deathParticle;

    EnemyWeaponController enemyWeaponController;
    PlayerHealth playerHealth;

    float advNearStateStopDist = 3f;
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
        combatDistance = enemyWeaponController.WeaponRange;
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        firstAttack = false;

    }

    protected override void OnDisable()
    {
        base.OnDisable();
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
        Strategy strategy = DetermineStrategy();
        Vector3 coveringPosition = transform.position;
        agent.isStopped = false;
        agent.speed = enemyBodySO.MoveSpeed * enemyWeaponController.CombatStatePenalty;
        agent.updateRotation = false;
        float actionTimer = 0f;


        bool isChasing = false;
        isActing = true;

        if (firstAttack)
        {
            Debug.Log("기습공격");

            if (playerTransform != null)
            {
                Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            animator.SetTrigger("ShootTrigger");

            lastAttacTime = Time.time;
            // yield return StartCoroutine(enemyWeaponController.FireBurst(playerTransform.position));

            firstAttack = false;
        }

        switch (strategy)
        {
            // 사거리 안 엄페물 찾기
            case Strategy.Equal:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.Near, 20f, combatDistance);
                if (coveringPosition == Vector3.zero)
                {
                    coveringPosition = playerTransform.position;
                    isChasing = true;
                }
                break;

            // 런이야
            case Strategy.DisAdvFar:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.FarPlayer);
                agent.updateRotation = true;
                agent.speed = enemyBodySO.MoveSpeed * 1.5f;
                break;

            // 약세 가까움 제자리에
            case Strategy.DisAdvNear:
                coveringPosition = transform.position;
                agent.isStopped = true;
                break;

            // 플레이어에 접근
            case Strategy.AdvFar:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.NearPlayer);
                if (coveringPosition == Vector3.zero)
                {
                    coveringPosition = playerTransform.position;
                    isChasing = true;
                }
                break;

            // 플레이어한테 이동.
            case Strategy.AdvNear:
                coveringPosition = playerTransform.position;
                isChasing = true;
                break;
        }

        Debug.Log($"{isChasing} 추노 모드 확인");

        // 커버링 전용 이동 로직
        if (!isChasing && !agent.isStopped && coveringPosition != Vector3.zero)
        {
            agent.SetDestination(coveringPosition);
        }

        float currentSpeed = agent.speed;

        while (actionTimer < 2.0f)
        {
            if (playerTransform == null) yield break;

            actionTimer += Time.deltaTime;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isShootAnim = stateInfo.IsTag("Shoot");

            if (isShootAnim)
            {
                agent.speed = currentSpeed * enemyWeaponController.ShootingPenalty;
            }
            else
            {
                agent.speed = currentSpeed;
            }

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

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            bool inRange = dist <= combatDistance;

            if (isChasing)
            {
                float stopDist = (strategy == Strategy.AdvNear) ? advNearStateStopDist : combatDistance * .5f;

                if (dist > stopDist)
                {
                    agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    agent.isStopped = true;
                }
            }

            if (Time.time >= lastAttacTime + enemyWeaponController.FireRate)
            {
                if ((strategy == Strategy.AdvNear || strategy == Strategy.DisAdvNear || sight.CanSeePlayer(playerTransform)) && inRange)
                {
                    animator.SetTrigger("ShootTrigger");
                    lastAttacTime = Time.time;

                    // Vector3 fireTarget = playerTransform.position;
                    // StartCoroutine(enemyWeaponController.FireBurst(fireTarget));
                }
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

    public void OnAnimationShoot()
    {
        if (isDead || playerTransform == null || !sight.CanSeePlayer(playerTransform)) return;
        StartCoroutine(enemyWeaponController.FireBurst(playerTransform.position));
    }

    protected override void OnDeath()
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
}


