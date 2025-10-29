using System;
using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera deathVirtualCam;
    [SerializeField] Transform weaponCamera;
    [SerializeField] Image[] shieldBars;
    [Range(1, 10)]
    [SerializeField] int startingHealth = 10;
    [SerializeField] GameObject gameOverContainer;
    int currentHitPoint;
    int gameOverVirtualCameraPrioity = 20;
    void Awake()
    {
        currentHitPoint = startingHealth;
        AdJustShieldUI();
    }

    public void TakeDamage(int amount)
    {
        currentHitPoint -= amount;
        AdJustShieldUI();

        if (currentHitPoint <= 0)
        {
            PlayerGameOver();
        }
    }

    void PlayerGameOver()
    {
        weaponCamera.parent = null;
        deathVirtualCam.Priority = gameOverVirtualCameraPrioity;
        gameOverContainer.SetActive(true);
        StarterAssetsInputs starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();
        starterAssetsInputs.SetCursorState(false);
        Destroy(this.gameObject);
    }

    void AdJustShieldUI()
    {
        for (int i = 0; i < shieldBars.Length; i++)
        {
            shieldBars[i].enabled = (i < currentHitPoint);
        }
    }
}
