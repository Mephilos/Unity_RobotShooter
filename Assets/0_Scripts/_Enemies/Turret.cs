using Unity.Mathematics;
using UnityEngine;

public class Turret : Enemy
{
    [SerializeField] Transform turretHead;
    [SerializeField] Transform projectileFirePoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject deathParticle;
    [SerializeField] float fireInterval = 5f;
    [SerializeField] float attackRange = 10f;
    [SerializeField] int damage = 2;
    [SerializeField] float turnSpeedHead = 2f;

    Quaternion originRotation;
    float currentTurnSpeed;
    float lastFire;

    protected override void Awake()
    {
        base.Awake();
        originRotation = turretHead.rotation;
        currentTurnSpeed = turnSpeedHead;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        enemyHealth.OnDeath += Death;
        lastFire = Time.time;
    }

    protected override void Update()
    {
        // if (playerTarget != null) turretHead.LookAt(playerTarget);
        base.Update();
    }

    protected override void OnDisable()
    {
        enemyHealth.OnDeath -= Death;
    }

    protected override bool IsTargetInRange(float dist)
    {
        return dist <= attackRange;
    }

    protected override void OnSpeedChange(float speedFactor)
    {
        currentTurnSpeed = turnSpeedHead * speedFactor;
    }

    protected override void Move()
    {
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, originRotation, Time.deltaTime * currentTurnSpeed);
    }

    protected override void TryAttack()
    {
        Vector3 direction = playerTarget.position - turretHead.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, lookRotation, Time.deltaTime * currentTurnSpeed);
        // turretHead.LookAt(playerTarget);

        if (Time.time >= lastFire + fireInterval)
        {
            Fire();
            lastFire = Time.time;
        }
    }

    void Fire()
    {
        Vector3 dir = (playerTarget.position - projectileFirePoint.position);
        Quaternion lookRotation = Quaternion.LookRotation(dir);

        GameObject projectile = PoolManager.Instance.Get(projectilePrefab, projectileFirePoint.position, lookRotation);
        Projectile newProjectile = projectile.GetComponent<Projectile>();
        newProjectile.Initialize(damage);
    }

    void Death()
    {
        Instantiate(deathParticle, turretHead.position, quaternion.identity);
        // PoolManager.Instance.Get(deathParticle, transform.position, quaternion.identity);

        Destroy(gameObject);
    }

}
