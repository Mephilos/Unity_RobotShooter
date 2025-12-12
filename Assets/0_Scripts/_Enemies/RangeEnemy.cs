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

    float advNearStateStopDist = 5f;
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
                float dist = Vector3.Distance(transform.position, playerTransform.position);

                if (dist > advNearStateStopDist)
                {
                    agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    agent.isStopped = true;
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
}


