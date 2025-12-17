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

    [SerializeField] protected EnemyBodySO enemyBodySO;
    [SerializeField] protected float combatDistance;
    [SerializeField] protected ParticleSystem deathParticle;

    protected EnemyWeaponController enemyWeaponController;
    protected float advNearStateStopDist = 3f;

    public EnemyWeaponController EnemyWeaponController => enemyWeaponController;
    public float CombatDistance => combatDistance;
    public EnemyBodySO EnemyBodySO => enemyBodySO;
    public float AdvNearStateStopDist => advNearStateStopDist;
    public bool FirstAttack { get; set; } = false;
    public float LastAttacTime { get; set; }


    protected override void Awake()
    {
        base.Awake();
        enemyWeaponController = GetComponent<EnemyWeaponController>();

        sight.viewDistance = enemyBodySO.DetectionRadius;
        sight.viewAngle = enemyBodySO.ViewAngle;
    }

    protected override void InitailizeState()
    {
        base.InitailizeState();
        CombatState = new RangeCombatState(this);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        agent.speed = enemyBodySO.MoveSpeed;
        enemyHealth.InitializeHealth(enemyBodySO.MaxHP);
        combatDistance = enemyWeaponController.WeaponRange;
        playerHealth = GameManager.Instance.Player;
        FirstAttack = false;

    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void OnFound()
    {
        base.OnFound();
        FirstAttack = true;
    }

    public virtual Strategy DetermineStrategy()
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

    public void GetFocusTarget(Vector3 target) => FocusTarget(target);
    protected virtual void FocusTarget(Vector3 target)
    {
        Vector3 lookDir = (target - transform.position).normalized;
        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
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

    protected override IEnumerator CombatRoutine()
    {
        yield break;
    }
}


