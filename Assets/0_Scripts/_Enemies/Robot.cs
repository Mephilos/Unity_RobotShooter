using UnityEngine;
using UnityEngine.AI;

public class Robot : Enemy
{
    [SerializeField] float moveSpeed = 3.5f;
    [SerializeField] GameObject deathParticle;

    NavMeshAgent agent;
    float backupSpeed;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        backupSpeed = moveSpeed;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
        agent.speed = backupSpeed;
        agent.Warp(transform.position);

        enemyHealth.OnDeath += SelfDestruct;

    }

    protected override void Update()
    {
        if (playerTransform == null) return;

        Move();
    }

    protected override void OnDisable()
    {
        enemyHealth.OnDeath -= SelfDestruct;
    }

    protected override void OnSpeedChange(float speedFactor)
    {
        agent.speed = backupSpeed * speedFactor;
        agent.velocity = agent.velocity.normalized * agent.speed;
    }

    protected override void Move()
    {
        if (playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    protected override bool IsTargetInRange(float dist) => false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Constants.PLAYER_TAG)) return;
        enemyHealth.TakeDamageProcess(Constants.ROBOT_SELF_DESTRUCT, false, false);
    }

    void SelfDestruct()
    {
        // Instantiate(deathParticle, transform.position, Quaternion.identity);
        PoolManager.Instance.Get(deathParticle, transform.position, Quaternion.identity);
        PoolManager.Instance.Release(gameObject);
    }

    protected override void TryAttack() { }
}
