using Unity.Mathematics;
using UnityEngine;

public class Turret : Enemy
{
    [SerializeField] Transform turretHead;
    [SerializeField] Transform projectileFirePoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject deathParticle;
    [SerializeField] float fireInterval = 3f;
    [SerializeField] float attackRange = 20f;
    [SerializeField] int damage = 2;

    float lastFire;

    protected override void Awake()
    {
        base.Awake();
        // lastFire = -fireInterval;
    }
    void OnEnable()
    {
        enemyHealth.OnDeath += Death;
    }
    protected override void Update()
    {
        base.Update();

        if (playerTarget != null) turretHead.LookAt(playerTarget);
    }
    void OnDisable()
    {
        enemyHealth.OnDeath -= Death;
    }

    protected override bool IsTargetInRange(float dist)
    {
        return dist <= attackRange;
    }

    protected override void Move() { }

    protected override void TryAttack()
    {
        if (Time.time >= lastFire + fireInterval)
        {
            Fire();
            lastFire = Time.time;
        }
    }
    void Fire()
    {
        Projectile newProjectile = Instantiate(projectilePrefab, projectileFirePoint.position, Quaternion.identity).GetComponent<Projectile>();
        newProjectile.transform.LookAt(playerTarget); // 발사된 투사체가 플레이어를 보고 전진하도록
        newProjectile.Initialize(damage);
    }
    void Death()
    {
        Instantiate(deathParticle, turretHead.position, quaternion.identity);
        Destroy(gameObject);
    }
}
