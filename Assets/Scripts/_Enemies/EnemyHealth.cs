using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int hitPoint = 3;
    [SerializeField] GameObject deathParticle;
    [SerializeField] int scoreValue = 100;
    [SerializeField] int weakPointKillBonus = 50;
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
    public void TakeDamage(int amount, bool isWeakPoint = false)
    {
        // TODO: 죽음 조건 분기 추가
        currentHitPoint -= amount;

        if (currentHitPoint <= 0)
        {
            SelfDestruct();
            int finalScore = scoreValue + (isWeakPoint ? weakPointKillBonus : 0);
            ScoreManager.Instance.AddScore(finalScore);
        }
    }

    public void SelfDestruct()
    {
        Instantiate(deathParticle, transform.position, Quaternion.identity);
        levelManager.AdjustEnemiesLeft(-1);
        Destroy(gameObject);
    }
}
