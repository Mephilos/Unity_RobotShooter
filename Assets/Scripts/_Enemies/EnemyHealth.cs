using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] int hitPoint = 3;
    [SerializeField] int scoreValue = 100;
    [SerializeField] int weakPointKillBonus = 50;
    LevelManager levelManager;
    int currentHitPoint;
    bool isDead;
    public event Action OnDeath;

    void Awake()
    {
        currentHitPoint = hitPoint;
    }

    void Start()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        levelManager.AdjustEnemiesLeft(1);
    }

    public void TakeDamage(int damage, Vector3 hitPoint, DamageType type)
    {
        TakeDamageProcess(damage, false);
    }

    public void TakeDamageProcess(int amount, bool isWeakPoint = false)
    {
        if (isDead) return;

        currentHitPoint -= amount;

        if (currentHitPoint <= 0)
        {
            Die(isWeakPoint);
        }
    }

    void Die(bool isWeakPoint)
    {
        isDead = true;
        int finalScore = scoreValue + (isWeakPoint ? weakPointKillBonus : 0);
        ScoreManager.Instance.AddScore(finalScore);
        levelManager.AdjustEnemiesLeft(-1);

        OnDeath?.Invoke();
    }

}
