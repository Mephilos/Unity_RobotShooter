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
    Quaternion originRotation;

    float lastFire;

    protected override void Awake()
    {
        base.Awake();
        originRotation = turretHead.rotation;
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

    void OnDisable()
    {
        enemyHealth.OnDeath -= Death;
    }

    protected override bool IsTargetInRange(float dist)
    {
        return dist <= attackRange;
    }

    protected override void Move()
    {
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, originRotation, Time.deltaTime * 2f);
    }

    protected override void TryAttack()
    {
        turretHead.LookAt(playerTarget);

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
