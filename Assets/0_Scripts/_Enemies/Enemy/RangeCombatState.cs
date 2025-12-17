using UnityEngine;
using System.Collections;

public class RangeCombatState : BaseEnemyState
{
    protected RangeEnemy range;
    public RangeCombatState(RangeEnemy range) : base(range)
    {
        this.range = range;
    }

    public override void Enter()
    {
        range.Animator.SetBool("IsCombat", true);
        range.StartStateCorutine(CombatRoutine());
    }

    public override void Execute()
    {
    }

    public override void Exit()
    {
        range.Animator.SetBool("IsCombat", false);
    }

    protected virtual IEnumerator CombatRoutine()
    {
        RangeEnemy.Strategy strategy = range.DetermineStrategy();
        Vector3 coveringPosition = range.transform.position;
        Transform playerTransform = range.PlayerTransform;

        range.Agent.isStopped = false;
        range.Agent.speed = range.EnemyBodySO.MoveSpeed * range.EnemyWeaponController.CombatStatePenalty;
        if (range.Agent.updateRotation) range.Agent.updateRotation = false;

        bool isChasing = false;


        if (range.FirstAttack)
        {
            Debug.Log("기습공격");

            if (playerTransform != null)
            {
                Vector3 lookDir = (playerTransform.position - range.transform.position).normalized;
                range.GetFocusTarget(lookDir);
            }

            range.Animator.SetTrigger("ShootTrigger");

            range.LastAttacTime = Time.time;
            // yield return StartCoroutine(enemyWeaponController.FireBurst(playerTransform.position));

            range.FirstAttack = false;
        }

        switch (strategy)
        {
            // 사거리 안 엄페물 찾기
            case RangeEnemy.Strategy.Equal:
                coveringPosition = range.Tactic.FindCover(playerTransform, EnemyTactic.Covering.Near, 20f, range.CombatDistance);
                if (coveringPosition == Vector3.zero)
                {
                    coveringPosition = playerTransform.position;
                    isChasing = true;
                }
                break;

            // 런이야
            case RangeEnemy.Strategy.DisAdvFar:
                coveringPosition = range.Tactic.FindCover(playerTransform, EnemyTactic.Covering.FarPlayer);
                range.Agent.updateRotation = true;
                range.Agent.speed = range.EnemyBodySO.MoveSpeed * 1.5f;
                break;

            // 약세 가까움 제자리에
            case RangeEnemy.Strategy.DisAdvNear:
                coveringPosition = range.transform.position;
                range.Agent.isStopped = true;
                break;

            // 플레이어에 접근
            case RangeEnemy.Strategy.AdvFar:
                coveringPosition = range.Tactic.FindCover(playerTransform, EnemyTactic.Covering.NearPlayer);
                if (coveringPosition == Vector3.zero)
                {
                    coveringPosition = playerTransform.position;
                    isChasing = true;
                }
                break;

            // 플레이어한테 이동.
            case RangeEnemy.Strategy.AdvNear:
                coveringPosition = playerTransform.position;
                isChasing = true;
                break;
        }

        Debug.Log($"{isChasing} 추노 모드 확인");

        // 커버링 전용 이동 로직
        if (!isChasing && !range.Agent.isStopped && coveringPosition != Vector3.zero)
        {
            range.Agent.SetDestination(coveringPosition);
        }

        float currentSpeed = range.Agent.speed;
        float actionTimer = 0f;

        while (actionTimer < 2.0f)
        {
            if (playerTransform == null) yield break;

            actionTimer += Time.deltaTime;

            var stateInfo = range.Animator.GetCurrentAnimatorStateInfo(0);
            bool isShootAnim = stateInfo.IsTag("Shoot");

            if (isShootAnim)
            {
                range.Agent.speed = currentSpeed * range.EnemyWeaponController.ShootingPenalty;
            }
            else
            {
                range.Agent.speed = currentSpeed;
            }

            if (playerTransform != null && strategy != RangeEnemy.Strategy.DisAdvFar)
            {
                Vector3 lookTarget = range.Sight.CanSeePlayer(playerTransform) ? playerTransform.position : range.LastPlayerPosition;
                range.GetFocusTarget(lookTarget);
            }

            float dist = Vector3.Distance(range.transform.position, playerTransform.position);
            bool inRange = dist <= range.CombatDistance;

            if (isChasing)
            {
                float stopDist = (strategy == RangeEnemy.Strategy.AdvNear) ? range.AdvNearStateStopDist : range.CombatDistance * .5f;

                if (dist > stopDist)
                {
                    range.Agent.isStopped = false;
                    range.Agent.SetDestination(playerTransform.position);
                }
                else
                {
                    range.Agent.isStopped = true;
                }
            }

            if (Time.time >= range.LastAttacTime + range.EnemyWeaponController.FireRate)
            {
                if ((strategy == RangeEnemy.Strategy.AdvNear || strategy == RangeEnemy.Strategy.DisAdvNear || range.Sight.CanSeePlayer(playerTransform)) && inRange)
                {
                    range.Animator.SetTrigger("ShootTrigger");
                    range.LastAttacTime = Time.time;

                    // Vector3 fireTarget = playerTransform.position;
                    // StartCoroutine(enemyWeaponController.FireBurst(fireTarget));
                }
            }
            yield return null;
        }
        range.Agent.updateRotation = true;
    }
}
