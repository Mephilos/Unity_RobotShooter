using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

public class PlayerOverlayHandler : MonoBehaviour
{
    [SerializeField] float damageFlashSpeed = 2f;
    [SerializeField] int gameOverVirtualCameraPrioity = 20;
    CinemachineCamera deathVirtualCamera;
    Camera weaponCamera;
    Image damageOverlay;
    Image[] shieldBar;
    GameObject gameOverContainer;
    bool isDeathMatch;
    PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnEnable()
    {
        playerHealth.OnHealthChanged += UpdateShieldUI;
        playerHealth.OnPlayerHit += PlayerDamageFlash;
        playerHealth.OnPlayerDeath += PlayerGameOver;
    }
    void OnDisable()
    {
        playerHealth.OnHealthChanged -= UpdateShieldUI;
        playerHealth.OnPlayerHit -= PlayerDamageFlash;
        playerHealth.OnPlayerDeath -= PlayerGameOver;
    }
    public void SetupOverlay(CinemachineCamera cinemachine, Camera weaponCamera, Image damageOverlay, Image[] shield, GameObject gameOverContainer, bool isDeathMatch)
    {
        this.deathVirtualCamera = cinemachine;
        this.weaponCamera = weaponCamera;
        this.damageOverlay = damageOverlay;
        this.shieldBar = shield;
        this.gameOverContainer = gameOverContainer;
        this.isDeathMatch = !isDeathMatch;
    }

    void UpdateShieldUI(int currentHp, int maxHp)
    {
        for (int i = 0; i < shieldBar.Length; i++)
        {
            shieldBar[i].enabled = (i < currentHp);
        }
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
    void PlayerGameOver()
    {
        weaponCamera.transform.parent = null;
        deathVirtualCamera.Priority = gameOverVirtualCameraPrioity;

        StarterAssetsInputs starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();
        starterAssetsInputs.SetInputBlocked(true);

        CursorManager.Instance.SetCursor(false);
        if (isDeathMatch)
        {
            gameOverContainer.SetActive(true);
        }
    }
}
