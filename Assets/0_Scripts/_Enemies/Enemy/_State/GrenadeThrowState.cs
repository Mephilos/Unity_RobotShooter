using System.Collections;
using UnityEngine;

public class GrenadeThrowState : BaseEnemyState
{
    private Grenadier grenadier;
    public GrenadeThrowState(Grenadier brain) : base(brain)
    {
        this.grenadier = brain;
    }

    public override void Enter()
    {
        grenadier.Agent.isStopped = true;
        grenadier.ThrowPosition = brain.LastPlayerPosition;
        grenadier.GetFocusTarget(grenadier.ThrowPosition);
        grenadier.Animator.SetTrigger("ThrowGrenade");
        grenadier.NextGrenadeTime = Time.time + grenadier.GrenadeCooltime;
        grenadier.StartStateCoroutine(ThrowGrenadeRoutine());
    }

    public override void Execute()
    {

    }
    public override void Exit()
    {

    }
    IEnumerator ThrowGrenadeRoutine()
    {
        yield return new WaitForSeconds(1.0f);

        grenadier.ChangeState(brain.SearchState);
    }
}
