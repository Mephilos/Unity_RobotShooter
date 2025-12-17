using UnityEngine;

public class EnemyPatrolState : BaseEnemyState
{
    public EnemyPatrolState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.Animator.SetBool("IsPatrol", true);
        brain.Agent.isStopped = false;
    }

    public override void Execute()
    {
        if (!brain.Agent.pathPending && brain.Agent.remainingDistance < 0.5f)
        {
            if (!brain.Agent.updateRotation) brain.Agent.updateRotation = true;

            Vector3 randomPoint = brain.Tactic.GetRandomWayPoint(brain.transform.position, 10f);
            brain.Agent.SetDestination(randomPoint);
        }
    }

    public override void Exit()
    {
        brain.Animator.SetBool("IsPatrol", true);
    }
}
