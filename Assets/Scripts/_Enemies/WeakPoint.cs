using UnityEngine;

public class WeakPoint : MonoBehaviour, IDamageable
{
    [SerializeField] EnemyHealth enemyHealth;
    [SerializeField] float damageMultiplier = 2.0f;

    void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    public void TakeDamage(int damage, Vector3 hitPoint, DamageType type)
    {
        int critical = Mathf.RoundToInt(damage * damageMultiplier);
        enemyHealth.TakeDamageProcess(critical, true);
    }
}
