using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class EnemyBrain : Enemy
{
    public enum AIState { Patrol, Combat, Search }
    [SerializeField] protected AIState currentState;
    [SerializeField] protected float combatTime = 5f;

    protected NavMeshAgent agent;
    protected EnemySight sight;
    protected EnemyTactic tactic;
    protected Vector3 lastPlayerPosition;
    protected float combatDuration;
    protected bool isActing = false;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<EnemySight>();
        tactic = GetComponent<EnemyTactic>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        currentState = AIState.Patrol;
        combatDuration = 0;
    }

    protected override void Update()
    {
        if (playerTransform == null) return;

        if (sight.CanSeePlayer(playerTransform))
        {
            lastPlayerPosition = playerTransform.position;

            if (currentState != AIState.Combat)
            {
                OnFound();
                SwitchingCombatMode();
            }
        }
        else if (currentState == AIState.Combat)
        {
            // 전투 초기화
            combatDuration += Time.deltaTime;
            if (combatDuration > combatTime)
            {
                SwitchingSearchMode();
                return;
            }
        }

        animator.SetBool("IsPatrol", false);
        animator.SetBool("IsCombat", false);
        animator.SetBool("IsSearch", false);

        switch (currentState)
        {
            case AIState.Patrol:
                animator.SetBool("IsPatrol", true);
                UpdatePatrol();
                break;
            case AIState.Combat:
                animator.SetBool("IsCombat", true);
                UpdateCombat();
                break;
            case AIState.Search:
                animator.SetBool("IsSearch", true);
                UpdateSearch();
                break;
        }
    }
    protected override void OnDamage()
    {
        if (currentState == AIState.Combat) return;

        StopAllCoroutines();
        isActing = false;

        base.OnDamage();

        Vector3 lookDir = (playerTransform.position - transform.position).normalized;
        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        if (currentState != AIState.Combat)
        {
            Debug.Log("기습당함");
            currentState = AIState.Combat;
            lastPlayerPosition = playerTransform.position;
            combatDuration = 0;
        }
    }
    protected virtual void UpdatePatrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!agent.updateRotation) agent.updateRotation = true;

            Vector3 ramdomPoint = tactic.GetRandomWayPoint(transform.position, 10f);
            agent.SetDestination(ramdomPoint);
        }
    }
    protected virtual void UpdateCombat()
    {
        if (isActing) return;
        StartCoroutine(CombatRoutine());
    }
    protected virtual void UpdateSearch()
    {
        Debug.LogWarning("서치모드");

        if (sight.CanSeePlayer(playerTransform))
        {
            SwitchingCombatMode();
            return;
        }

        if (!isActing)
        {
            StartCoroutine(SearchRoutine());
        }
    }

    protected abstract IEnumerator CombatRoutine();
    protected virtual IEnumerator SearchRoutine()
    {
        isActing = true;
        agent.isStopped = false;

        if (agent.updateRotation) agent.updateRotation = false;

        int wayPointCounter = Random.Range(1, 3);

        for (int i = 0; i < wayPointCounter; i++)
        {
            Vector3 bluffPosition = tactic.GetRandomWayPoint(lastPlayerPosition, 10f);

            agent.SetDestination(bluffPosition);

            while (agent.pathPending || agent.remainingDistance > .5f)
            {
                if (sight.CanSeePlayer(playerTransform))
                {
                    isActing = false;
                    yield break;
                }
                Vector3 lookDir = (lastPlayerPosition - transform.position).normalized;
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);
                }
                yield return null;
            }
        }

        agent.SetDestination(lastPlayerPosition);

        while (agent.pathPending || agent.remainingDistance > 0.5f)
        {
            if (sight.CanSeePlayer(playerTransform))
            {
                isActing = false;
                yield break;
            }
            yield return null;
        }

        Debug.Log("수색 모드에서 정찰 모드로 전환");
        currentState = AIState.Patrol;
        isActing = false;
    }
    protected void SwitchingCombatMode()
    {
        currentState = AIState.Combat;
        combatDuration = 0;
        StopAllCoroutines();
        isActing = false;
    }
    protected void SwitchingSearchMode()
    {
        Debug.Log("전투종료 수색 ㄱㄱ");
        currentState = AIState.Search;
        agent.ResetPath();
        isActing = false;
    }
    protected virtual void OnFound() { }
}
