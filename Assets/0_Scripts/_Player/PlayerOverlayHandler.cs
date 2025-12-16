using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using TMPro;

public class PlayerOverlayHandler : MonoBehaviour
{
    [SerializeField] float damageFlashSpeed = 2f;
    [SerializeField] int gameOverVirtualCameraPrioity = 20;
    [SerializeField] ActiveWeapon activeWeapon;
    [SerializeField] Transform cameraRoot;
    CinemachineCamera deathVirtualCamera;
    Image damageOverlay;
    PlayerHealth playerHealth;
    public Transform CameraRoot => cameraRoot;
    public ActiveWeapon ActiveWeapon => activeWeapon;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnEnable()
    {
        playerHealth.OnPlayerHit += PlayerDamageFlash;
        playerHealth.OnPlayerDeath += DeathCameraMove;
    }

    void OnDisable()
    {
        playerHealth.OnPlayerHit -= PlayerDamageFlash;
        playerHealth.OnPlayerDeath -= DeathCameraMove;
    }

    public void SetupOverlay(CinemachineCamera cinemachine, Image damageOverlay)
    {
        this.deathVirtualCamera = cinemachine;
        this.damageOverlay = damageOverlay;
    }

    void PlayerDamageFlash()
    {
        StartCoroutine(DamageFlashRoutine());
    }

    IEnumerator DamageFlashRoutine()
    {
        Color color = damageOverlay.color;
        color.a = 0.8f;
        damageOverlay.color = color;

        while (damageOverlay.color.a > 0)
        {
            color.a -= Time.deltaTime * damageFlashSpeed;
            damageOverlay.color = color;
            yield return null;
        }
    }
    void DeathCameraMove()
    {
        deathVirtualCamera.Priority = gameOverVirtualCameraPrioity;
    }
}
