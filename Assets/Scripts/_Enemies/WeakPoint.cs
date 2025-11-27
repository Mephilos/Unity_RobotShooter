using UnityEngine;

public class WeakPoint : MonoBehaviour
{
    [SerializeField] EnemyHealth enemyHealth;
    [SerializeField] float damageMultiplier = 2.0f;

    void Start()
    {
        enemyHealth = GetComponentInParent<EnemyHealth>();
    }
    public void OnHit(int damage)
    {
        int critical = Mathf.RoundToInt(damage * damageMultiplier);

        enemyHealth.TakeDamage(critical, true);
    }
}
