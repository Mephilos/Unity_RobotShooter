using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Cinemachine;
using System;
using UnityEngine.Rendering.Universal;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] bool isDeathMatchMode = false;

    [SerializeField] GameObject playerPrefab;
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float respawnDelay = 4f;

    [SerializeField] CinemachineCamera playerFollowCam;
    [SerializeField] CinemachineCamera deathCam;
    [SerializeField] Image damageOverlay;
    [SerializeField] Image[] shieldBars;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] GameObject zoomUI;

    public event Action OnPlayerDeath;

    void Start()
    {
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
        Camera[] allCameras = playerObject.GetComponentsInChildren<Camera>(true);
        Camera weaponCamera = null;

        foreach (var cameraComponent in allCameras)
        {
            var cameraData = cameraComponent.GetUniversalAdditionalCameraData();

            if (cameraData.renderType == CameraRenderType.Overlay || cameraComponent.name.Contains("Weapon"))
            {
                weaponCamera = cameraComponent;
                break;
            }
        }

        if (weaponCamera != null)
        {
            var mainCameraData = Camera.main.GetUniversalAdditionalCameraData();
            if (!mainCameraData.cameraStack.Contains(weaponCamera))
            {
                mainCameraData.cameraStack.Add(weaponCamera);
            }
        }

        var activeWeapon = playerObject.GetComponentInChildren<ActiveWeapon>();
        PlaySceneUI playSceneUI = FindFirstObjectByType<PlaySceneUI>();

        if (playSceneUI != null && activeWeapon != null)
            playSceneUI.BindWeapon(activeWeapon);

        if (playerObject.TryGetComponent<PlayerHealth>(out var playerHealth))
        {
            playerHealth.SetupReferences(deathCam, damageOverlay, shieldBars, gameOverUI);

            if (isDeathMatchMode)
            {
                playerHealth.OnPlayerDeath += () => OnPlayerDeath?.Invoke();
            }

            GameManager.Instance.FindPlayer(playerHealth);
        }

        if (activeWeapon != null)
            activeWeapon.SetupReferences(playerFollowCam, zoomUI);

        if (playerFollowCam != null)
        {
            Transform cameraRoot = playerObject.transform.Find("PlayerCameraRoot");
            if (cameraRoot == null)
                cameraRoot = playerObject.transform;

            playerFollowCam.Follow = cameraRoot;
            playerFollowCam.LookAt = cameraRoot;
        }

        CursorManager.Instance.SetCursor(true);
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