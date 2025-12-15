using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;
using System;
using UnityEngine.Rendering.Universal;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] bool isDeathMatchMode = false;
    [SerializeField] bool autoSpawn = true;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float respawnDelay = 4f;

    [SerializeField] CinemachineCamera playerFollowCamera;
    [SerializeField] CinemachineCamera deathCam;
    [SerializeField] Image damageOverlay;
    [SerializeField] Image[] shieldBar;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] GameObject zoomUI;
    [SerializeField] Crosshair crosshair;

    public event Action OnPlayerDeath;

    void Start()
    {
        if (autoSpawn && !isDeathMatchMode)
            SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        int spawnIndex = isDeathMatchMode ? UnityEngine.Random.Range(0, spawnPoints.Length) : 0;
        spawnPosition = spawnPoints[spawnIndex].position;
        spawnRotation = spawnPoints[spawnIndex].rotation;

        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        InjectDependencies(playerInstance);
    }

    void InjectDependencies(GameObject playerObject)
    {
        Camera weaponCamera = playerObject.GetComponentInChildren<Camera>();

        var mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
        mainCameraData.cameraStack.Add(weaponCamera);

        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        PlayerOverlayHandler playerOverlay = playerObject.GetComponent<PlayerOverlayHandler>();
        ActiveWeapon activeWeapon = playerObject.GetComponentInChildren<ActiveWeapon>();
        playerOverlay.SetupOverlay(deathCam, weaponCamera, damageOverlay, shieldBar, gameOverUI, isDeathMatchMode);

        if (isDeathMatchMode)
        {
            // 핼스 에서 죽음 이밴트 날아가면 죽었다고 이벤트 날리기 누구한테 DeathMatchMode 한테.
            playerHealth.OnPlayerDeath += () => OnPlayerDeath?.Invoke();
        }
        GameManager.Instance.FindPlayer(playerHealth);

        PlaySceneUI playSceneUI = FindFirstObjectByType<PlaySceneUI>();
        playSceneUI.BindWeapon(activeWeapon);

        activeWeapon.SetupReferences(playerFollowCamera, zoomUI);

        Transform cameraRoot = playerObject.transform.Find("PlayerCameraRoot");
        playerFollowCamera.Follow = cameraRoot;
        playerFollowCamera.LookAt = cameraRoot;

        CursorManager.Instance.SetCursor(true);

        crosshair.Initialize(activeWeapon);
        playerHealth.Initialize();
    }

    public void RequestRespawn()
    {
        if (isDeathMatchMode)
            StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnPlayer();
    }
}