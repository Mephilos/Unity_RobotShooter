using UnityEngine;
using StarterAssets;
[RequireComponent(typeof(EnemyHealth))]
public abstract class Enemy : MonoBehaviour
{
    protected EnemyHealth enemyHealth;
    protected Transform playerTarget;

    protected virtual void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    protected virtual void Start()
    {
        var player = FindFirstObjectByType<FirstPersonController>();
        playerTarget = player.transform;
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
    protected abstract bool IsTargetInRange(float dist);
    protected abstract void Move();
    protected abstract void TryAttack();
}
