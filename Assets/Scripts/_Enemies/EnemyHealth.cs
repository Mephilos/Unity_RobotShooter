using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int hitPoint = 3;
    [SerializeField] GameObject deathParticle;
    LevelManager levelManager;
    int currentHitPoint;

    void Awake()
    {
        currentHitPoint = hitPoint;
    }

    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        levelManager.AdjustEnemiesLeft(1);
    }
    public void TakeDamage(int amount)
    {
        currentHitPoint -= amount;

        if (currentHitPoint <= 0)
        {
            levelManager.AdjustEnemiesLeft(-1);
            SelfDestruct();
        }
    }

    public void SelfDestruct()
    {
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
