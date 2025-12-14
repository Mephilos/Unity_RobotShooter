using UnityEngine;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealth))]
public abstract class Enemy : MonoBehaviour
{
    [SerializeField] protected float hisSlowDuration = 1f;
    [SerializeField] protected float hitSlowFactor = 0.1f;
    [SerializeField] MMF_Player hitFeedback;

    protected Animator animator;
    protected EnemyHealth enemyHealth;
    protected Transform playerTransform;
    protected PlayerHealth playerHealth;
    protected Coroutine slowRoutine;
    protected bool isDead;

    protected virtual void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
    }

    protected virtual void OnEnable()
    {
        isDead = false;
        if (TryGetComponent<NavMeshAgent>(out var agent))
        {
            agent.enabled = true;
            agent.Warp(transform.position);

            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
            }
        }

        var colliders = GetComponentsInChildren<Collider>();

        foreach (var c in colliders)
        {
            c.enabled = true;
        }

        if (GameManager.instance.Player == null)
        {
            playerHealth = null;
            playerTransform = null;
            StartCoroutine(WaitForPlayer());
        }
        else
        {
            playerHealth = GameManager.instance.Player;
            playerTransform = playerHealth.transform;
        }

        enemyHealth.OnHit += OnDamage;
        enemyHealth.OnDeath += HandleDeath;
    }
    IEnumerator WaitForPlayer()
    {
        while (GameManager.instance.Player == null)
        {
            yield return null;
        }

        playerHealth = GameManager.instance.Player;
        playerTransform = playerHealth.transform;
    }
    protected virtual void OnDisable()
    {
        enemyHealth.OnHit -= OnDamage;
    }

    protected virtual void Update()
    {
        if (playerTransform == null) return;

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);

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

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        if (GetComponent<NavMeshAgent>() != null)
        {
            var agent = GetComponent<NavMeshAgent>();

            agent.isStopped = true;
            agent.enabled = false;
        }

        var collider = GetComponentsInChildren<Collider>();
        foreach (var c in collider)
        {
            c.enabled = false;
        }

        OnDeath();
    }
    protected abstract void OnSpeedChange(float speedFactor);
    protected abstract bool IsTargetInRange(float dist);
    protected abstract void Move();
    protected abstract void TryAttack();
    protected abstract void OnDeath();
}
