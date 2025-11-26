using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int hitPoint = 3;
    [SerializeField] GameObject deathParticle;
    [SerializeField] int ScoreValue = 100;
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
        // TODO: 죽음 조건 분기 추가
        currentHitPoint -= amount;

        if (currentHitPoint <= 0)
        {
            SelfDestruct();
            ScoreManager.Instance.AddScore(ScoreValue);
        }
    }

    public void SelfDestruct()
    {
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        levelManager.AdjustEnemiesLeft(-1);
        Destroy(gameObject);
    }
}
