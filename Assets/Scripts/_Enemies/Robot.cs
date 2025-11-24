using UnityEngine;
using UnityEngine.AI;

public class Robot : Enemy
{
    [SerializeField] float moveSpeed = 3.5f;
    NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    protected override void Update()
    {
        if (playerTarget == null) return;

        Move();
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Constants.PLAYER_TAG)) return;
        enemyHealth.SelfDestruct();
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
}
