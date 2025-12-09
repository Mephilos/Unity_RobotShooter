using UnityEngine;
using UnityEngine.AI;

public class Robot : Enemy
{
    [SerializeField] float moveSpeed = 3.5f;
    [SerializeField] GameObject deathParticle;
    NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        enemyHealth.OnDeath += SelfDestruct;
        if (agent.isOnNavMesh)
            agent.ResetPath();
        agent.Warp(transform.position);
    }

    protected override void Update()
    {
        if (playerTarget == null) return;

        Move();
    }

    void OnDisable()
    {
        enemyHealth.OnDeath -= SelfDestruct;
    }

    protected override void Move()
    {
        if (playerTarget != null)
        {
            agent.SetDestination(playerTarget.position);
        }
    }
    protected override void TryAttack() { }
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
}
