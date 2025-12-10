using UnityEngine;
using StarterAssets;
using MoreMountains.Feedbacks;
using System;
using System.Collections;

[RequireComponent(typeof(EnemyHealth))]
public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected float hisSlowDuration = 1f;
    [SerializeField] protected float hitSlowFactor = 0.1f;
    [SerializeField] MMF_Player hitFeedback;

    protected EnemyHealth enemyHealth;
    protected Transform playerTarget;
    protected Coroutine slowRoutine;

    protected virtual void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    protected virtual void OnEnable()
    {
        var player = FindFirstObjectByType<FirstPersonController>();
        playerTarget = player.transform.Find(Constants.PLAYER_TARGET);
        enemyHealth.OnHit += OnDamage;
    }

    protected virtual void OnDisable()
    {
        enemyHealth.OnHit -= OnDamage;
    }

    protected virtual void Update()
    {
        if (playerTarget == null) return;

        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (IsTargetInRange(distToPlayer))
        {
            TryAttack();
        }
        else
        {
            Move();
        }
    }

    protected virtual void OnDamage()
    {
        // hitFeedback.PlayFeedbacks();
        Debug.Log($"{this.gameObject} 느려짐");

        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
        }

        slowRoutine = StartCoroutine(OnSlowRoutine());
    }

    IEnumerator OnSlowRoutine()
    {
        Debug.Log("슬로우 루틴 발동");
        OnSpeedChange(hitSlowFactor);

        yield return new WaitForSeconds(hisSlowDuration);

        OnSpeedChange(1.0f);
        slowRoutine = null;
    }

    protected abstract void OnSpeedChange(float speedFactor);
    protected abstract bool IsTargetInRange(float dist);
    protected abstract void Move();
    protected abstract void TryAttack();
}
