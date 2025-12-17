using System.Collections;
using UnityEngine;

public class EnemySearchState : BaseEnemyState
{
    public EnemySearchState(EnemyBrain brain) : base(brain) { }

    public override void Enter()
    {
        brain.Animator.SetBool("IsSearch", true);
        brain.Agent.isStopped = false;
        brain.StartStateCoroutine(SearchRoutine());
    }

    public override void Execute()
    {
    }

    public override void Exit()
    {
        brain.Animator.SetBool("IsSearch", false);
        brain.StopAllCoroutines(); // 서치 끝내기
    }

    IEnumerator SearchRoutine()
    {
        if (brain.Agent.updateRotation) brain.Agent.updateRotation = false;

        int wayPointCounter = Random.Range(1, 3);

        for (int i = 0; i < wayPointCounter; i++)
        {
            Vector3 bluffPosition = brain.Tactic.GetRandomWayPoint(brain.LastPlayerPosition, 10f);

            yield return brain.StartCoroutine(SearchMove(bluffPosition));
        }

        brain.Agent.SetDestination(brain.LastPlayerPosition);

        yield return brain.StartCoroutine(SearchMove(brain.LastPlayerPosition));

        Debug.Log("수색 모드에서 정찰 모드로 전환");
        brain.ChangeState(brain.PatrolState);
    }

    IEnumerator SearchMove(Vector3 targetPosition)
    {
        brain.Agent.SetDestination(targetPosition);

        while (brain.Agent.pathPending || brain.Agent.remainingDistance > .5f)
        {
            // 플레이어 발견시 brainUpdate에서 상태 변환 시킴 로직만
            Vector3 lookDir = (brain.LastPlayerPosition - brain.transform.position).normalized;
            lookDir.y = 0;
            if (lookDir != Vector3.zero)
            {
                brain.transform.rotation = Quaternion.Slerp(brain.transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
            }
            yield return null;
        }
    }
}
