using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int hitPoint = 3;

    int currentHitPoint;

    void Awake()
    {
        currentHitPoint = hitPoint;
    }
    public void TakeDamage(int amount)
    {
        currentHitPoint -= amount;

        if (currentHitPoint <= 0)
        {
            Destroy(gameObject);
        }
    }
}
