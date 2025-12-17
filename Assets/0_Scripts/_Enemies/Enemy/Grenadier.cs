using System.Collections;
using UnityEngine;

public class Grenadier : RangeEnemy
{
    [SerializeField] GameObject grenadePrefab;
    [SerializeField] Transform grenadeFirePoint;
    [SerializeField] float grenadeCooltime = 10f;
    [SerializeField] float throwAngle = 45f;
    [SerializeField] float throwRangeMin = 5f;
    [SerializeField] float throwRangeMax = 20f;
    [SerializeField] LayerMask findRouteLayer;

    Vector3 throwPosition;
    float nextGrenadeTime;
    bool isThrowing = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        nextGrenadeTime = Time.time + grenadeCooltime;
    }

    protected override IEnumerator CombatRoutine()
    {
        Strategy strategy = DetermineStrategy();
        Vector3 coveringPosition = transform.position;

        agent.isStopped = false;
        agent.speed = enemyBodySO.MoveSpeed * enemyWeaponController.CombatStatePenalty;
        if (agent.updateRotation) agent.updateRotation = false;

        float currentSpeed = agent.speed;
        float actionTimer = 0f;

        bool isChasing = false;
        isActing = true;

        if (firstAttack)
        {
            Debug.Log("기습공격");

            if (playerTransform != null)
            {
                Vector3 lookDir = (playerTransform.position - transform.position).normalized;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);
            }

            animator.SetTrigger("ShootTrigger");

            lastAttacTime = Time.time;
            // yield return StartCoroutine(enemyWeaponController.FireBurst(playerTransform.position));

            firstAttack = false;
        }

        switch (strategy)
        {
            // 사거리 안 엄페물 찾기
            case Strategy.Equal:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.Near, 20f, combatDistance);
                if (coveringPosition == Vector3.zero)
                {
                    coveringPosition = playerTransform.position;
                    isChasing = true;
                }
                break;

            // 런이야
            case Strategy.DisAdvFar:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.FarPlayer);
                agent.updateRotation = true;
                agent.speed = enemyBodySO.MoveSpeed * 1.5f;
                break;

            // 약세 가까움 제자리에
            case Strategy.DisAdvNear:
                coveringPosition = transform.position;
                agent.isStopped = true;
                break;

            // 플레이어에 접근
            case Strategy.AdvFar:
                coveringPosition = tactic.FindCover(playerTransform, EnemyTactic.Covering.NearPlayer);
                if (coveringPosition == Vector3.zero)
                {
                    coveringPosition = playerTransform.position;
                    isChasing = true;
                }
                break;

            // 플레이어한테 이동.
            case Strategy.AdvNear:
                coveringPosition = playerTransform.position;
                isChasing = true;
                break;
        }

        Debug.Log($"{isChasing} 추노 모드 확인");

        // 커버링 전용 이동 로직
        if (!isChasing && !agent.isStopped && coveringPosition != Vector3.zero)
        {
            agent.SetDestination(coveringPosition);
        }

        while (actionTimer < 2.0f)
        {
            if (playerTransform == null) yield break;

            actionTimer += Time.deltaTime;

            if (isThrowing)
            {
                agent.isStopped = true;
                yield return null;
                continue;
            }

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            bool isShootAnim = stateInfo.IsTag("Shoot");

            if (isShootAnim)
            {
                agent.speed = currentSpeed * enemyWeaponController.ShootingPenalty;
            }
            else
            {
                agent.speed = currentSpeed;
            }

            if (playerTransform != null && strategy != Strategy.DisAdvFar)
            {
                Vector3 lookTarget = sight.CanSeePlayer(playerTransform) ? playerTransform.position : lastPlayerPosition;
                FocusTarget(lookTarget);
            }

            float dist = Vector3.Distance(transform.position, playerTransform.position);
            bool inRange = dist <= combatDistance;


            if (CanThrowGrenade(dist))
            {
                yield return StartCoroutine(ThrowGrenadeRoutine(playerTransform.position));
                continue;
            }

            if (isChasing)
            {
                float stopDist = (strategy == Strategy.AdvNear) ? advNearStateStopDist : combatDistance * .5f;

                if (dist > stopDist)
                {
                    agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    agent.isStopped = true;
                }
            }

            if (Time.time >= lastAttacTime + enemyWeaponController.FireRate)
            {
                if ((strategy == Strategy.AdvNear || strategy == Strategy.DisAdvNear || sight.CanSeePlayer(playerTransform)) && inRange)
                {
                    animator.SetTrigger("ShootTrigger");
                    lastAttacTime = Time.time;

                    // Vector3 fireTarget = playerTransform.position;
                    // StartCoroutine(enemyWeaponController.FireBurst(fireTarget));
                }
            }
            yield return null;
        }
        agent.updateRotation = true;
        isActing = false;
    }

    protected override IEnumerator SearchRoutine()
    {
        isActing = true;
        agent.isStopped = false;

        if (agent.updateRotation) agent.updateRotation = false;

        int wayPointCounter = Random.Range(1, 3);

        for (int i = 0; i < wayPointCounter; i++)
        {
            Vector3 bluffPosition = tactic.GetRandomWayPoint(lastPlayerPosition, 10f);

            yield return StartCoroutine(SearchMove(bluffPosition));
            if (!isActing) yield break;
        }

        agent.SetDestination(lastPlayerPosition);

        yield return StartCoroutine(SearchMove(lastPlayerPosition));

        Debug.Log("수색 모드에서 정찰 모드로 전환");
        currentState = AIState.Patrol;
        isActing = false;
    }

    protected override IEnumerator SearchMove(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);

        while (agent.pathPending || agent.remainingDistance > .5f)
        {
            if (sight.CanSeePlayer(playerTransform))
            {
                isActing = false;
                yield break;
            }

            float dist = Vector3.Distance(transform.position, lastPlayerPosition);

            if (CanThrowGrenade(dist))
            {
                yield return StartCoroutine(ThrowGrenadeRoutine(lastPlayerPosition));
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
    public void OnAnimationThrow()
    {
        if (!isThrowing || isDead) return;

        GameObject grenade = PoolManager.Instance.Get(grenadePrefab, grenadeFirePoint.position, Quaternion.identity);
        var newGrenade = grenade.GetComponent<Grenade>();

        Vector3 velocity = CalculateVelocity(throwPosition, grenadeFirePoint.position, throwAngle);
        newGrenade.Throw(velocity);
    }
    bool CanThrowGrenade(float dist)
    {
        if (Time.time < nextGrenadeTime || isThrowing) return false;
        if (dist < throwRangeMin || dist > throwRangeMax) return false;
        if (Physics.Raycast(grenadeFirePoint.position, Vector3.up, 2f, findRouteLayer)) return false;
        return true;
    }

    IEnumerator ThrowGrenadeRoutine(Vector3 targetPosition)
    {
        isThrowing = true;
        agent.isStopped = true;
        throwPosition = targetPosition;

        FocusTarget(targetPosition);
        animator.SetTrigger("ThrowGrenade");

        nextGrenadeTime = Time.time + grenadeCooltime;

        yield return new WaitForSeconds(1.0f);

        isThrowing = false;
        agent.isStopped = false;
    }

    Vector3 CalculateVelocity(Vector3 target, Vector3 start, float angle)
    {
        Vector3 direction = target - start;
        float height = direction.y;
        direction.y = 0;
        float dist = direction.magnitude;
        float a = angle * Mathf.Deg2Rad;

        direction.y = dist * Mathf.Tan(a);
        dist += Mathf.Abs(height / Mathf.Tan(a));

        float vel = Mathf.Sqrt(dist * Physics.gravity.magnitude / Mathf.Sin(2 * a));
        return vel * direction.normalized;
    }
}

