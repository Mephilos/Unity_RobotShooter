using System;
using Unity.Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] CinemachineCamera deathVirtualCam;
    [SerializeField] Transform weaponCamera;
    [SerializeField] Image damageOverlay;
    [SerializeField] float damageFlashSpeed = 2f;
    [SerializeField] Image[] shieldBars;
    [Range(1, 10)]
    [SerializeField] int startingHealth = 10;
    [SerializeField] GameObject gameOverContainer;
    [SerializeField] bool invincibleMode = false;
    int currentHitPoint;
    int gameOverVirtualCameraPrioity = 20;

    void Awake()
    {
        currentHitPoint = startingHealth;
        AdJustShieldUI();
    }

    void Update()
    {
        Invincible(invincibleMode);
    }

    public void TakeDamage(int amount)
    {
        currentHitPoint -= amount;
        AdJustShieldUI();

        StartCoroutine(DamageFlashRoutine());
        if (currentHitPoint <= 0)
        {
            PlayerGameOver();
        }
    }

    IEnumerator DamageFlashRoutine()
    {
        Color color = damageOverlay.color;
        color.a = 0.8f; // 순간적으로 빨갛게
        damageOverlay.color = color;

        while (damageOverlay.color.a > 0)
        {
            color.a -= Time.deltaTime * damageFlashSpeed;
            damageOverlay.color = color;
            yield return null;
        }
    }

    void PlayerGameOver()
    {
        weaponCamera.parent = null;
        deathVirtualCam.Priority = gameOverVirtualCameraPrioity;
        gameOverContainer.SetActive(true);
        StarterAssetsInputs starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();
        starterAssetsInputs.SetInputBlocked(true);
        CursorManager.Instance.SetCursor(false);
        Destroy(this.gameObject);
    }

    void AdJustShieldUI()
    {
        for (int i = 0; i < shieldBars.Length; i++)
        {
            shieldBars[i].enabled = (i < currentHitPoint);
        }
    }

    void Invincible(bool invincibleMode)
    {
        if (!invincibleMode) return;
        currentHitPoint = 10000000;
    }
}
