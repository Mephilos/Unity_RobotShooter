using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera deathVirtualCam;
    [SerializeField] Transform weaponCamera;
    [SerializeField] Image[] shieldBars;
    [Range(1, 10)]
    [SerializeField] int startingHealth = 10;
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
            weaponCamera.parent = null;
            deathVirtualCam.Priority = gameOverVirtualCameraPrioity;
            Destroy(this.gameObject);
        }
    }

    void AdJustShieldUI()
    {
        for (int i = 0; i < shieldBars.Length; i++)
        {
            shieldBars[i].enabled = (i < currentHitPoint);
        }
    }
}
