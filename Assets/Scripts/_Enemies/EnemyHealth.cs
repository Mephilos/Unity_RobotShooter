using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int hitPoint = 3;
    [SerializeField] GameObject deathParticle;

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
            SelfDestruct();
        }
    }

    public void SelfDestruct()
    {
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
