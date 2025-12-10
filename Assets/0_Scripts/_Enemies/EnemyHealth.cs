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
    public event Action OnHit;

    public int CurrentHP => currentHitPoint;
    public int MaxHP => hitPoint;

    public void InitializeHealth(int HP)
    {
        hitPoint = HP;
        currentHitPoint = HP;
        isDead = false;
    }

    void OnEnable()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        currentHitPoint = hitPoint;
        isDead = false;

        levelManager.AdjustEnemiesLeft(1);
    }

    public void TakeDamage(int damage, Vector3 hitPoint, DamageType type)
    {
        TakeDamageProcess(damage, false);
    }

    public void TakeDamageProcess(int amount, bool isWeakPoint = false, bool giveScore = true)
    {
        if (isDead) return;

        currentHitPoint -= amount;

        OnHit?.Invoke();

        if (currentHitPoint <= 0)
        {
            Die(isWeakPoint, giveScore);
        }
    }

    void Die(bool isWeakPoint, bool giveScore)
    {
        isDead = true;
        if (giveScore)
        {
            int finalScore = scoreValue + (isWeakPoint ? weakPointKillBonus : 0);
            ScoreManager.Instance.AddScore(finalScore);
        }
        levelManager.AdjustEnemiesLeft(-1);

        OnDeath?.Invoke();
    }

}
