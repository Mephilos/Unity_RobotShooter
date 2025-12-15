using System;
using Unity.Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] Transform playerCameraRoot;
    [SerializeField] ActiveWeapon activeWeapon;
    [Range(1, 10)]
    [SerializeField] int startingHealth = 10;
    [SerializeField] bool invincibleMode = false;
    int currentHitPoint;
    public event Action OnPlayerDeath;
    public event Action OnPlayerHit;
    public event Action<int, int> OnHealthChanged;

    public int CurrentHP => currentHitPoint;
    public int MaxHP => startingHealth;
    public Transform CameraRoot => playerCameraRoot;
    public ActiveWeapon Weapon => activeWeapon;

    void Awake()
    {
        playerCameraRoot = transform.Find("PlayerCameraRoot");
        activeWeapon = GetComponentInChildren<ActiveWeapon>();
    }

    public void Initialize()
    {
        currentHitPoint = startingHealth;
        OnHealthChanged?.Invoke(startingHealth, startingHealth);
        GameManager.Instance.FindPlayer(this);
    }

    void Update()
    {
        Invincible(invincibleMode);
    }

    public void TakeDamage(int amount)
    {
        // if (invincibleMode) return;

        currentHitPoint -= amount;

        OnHealthChanged?.Invoke(currentHitPoint, startingHealth);
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
