using System;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int maxHp = 100;
    [SerializeField] bool invincibleMode = false;
    int currentHitPoint;
    public event Action OnPlayerDeath;
    public event Action OnPlayerHit;
    public event Action<int, int> OnHealthChanged;

    public int CurrentHP => currentHitPoint;
    public int MaxHP => maxHp;

    public void Initialize()
    {
        currentHitPoint = maxHp;
        OnHealthChanged?.Invoke(currentHitPoint, maxHp);
    }

    void Update()
    {
        Invincible(invincibleMode);
    }

    public void TakeDamage(int amount)
    {
        // if (invincibleMode) return;

        currentHitPoint -= amount;

        OnHealthChanged?.Invoke(currentHitPoint, maxHp);
        OnPlayerHit?.Invoke();

        if (currentHitPoint <= 0)
        {
            Die();
        }
    }
    void Die()
    {
        OnPlayerDeath?.Invoke();

        Destroy(gameObject);
    }

    void Invincible(bool invincibleMode)
    {
        if (!invincibleMode) return;
        currentHitPoint = 100;
    }
}
