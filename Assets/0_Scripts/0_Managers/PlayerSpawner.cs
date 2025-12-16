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
        if (mainCameraData.cameraStack.Count > 0)
        {
            mainCameraData.cameraStack.RemoveAll(cam => cam == null);
        }
        mainCameraData.cameraStack.Add(weaponCamera);

        PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
        PlayerOverlayHandler playerOverlay = playerObject.GetComponent<PlayerOverlayHandler>();
        ActiveWeapon activeWeapon = playerObject.GetComponentInChildren<ActiveWeapon>();
        Transform cameraRoot = playerOverlay.CameraRoot;


        deathCam.Priority = 0;
        if (isDeathMatchMode)
        {
            // 핼스 에서 죽음 이밴트 날아가면 죽었다고 이벤트 날리기 누구한테 DeathMatchMode 한테.
            playerHealth.OnPlayerDeath += () => OnPlayerDeath?.Invoke();
        }
        activeWeapon.SetupReferences(playerFollowCamera, zoomUI);
        playerOverlay.SetupOverlay(deathCam, damageOverlay);
        playerFollowCamera.Follow = cameraRoot;
        playerFollowCamera.LookAt = cameraRoot;

        activeWeapon.Initialize();
        playerHealth.Initialize();
        crosshair.Initialize(activeWeapon);
        CursorManager.Instance.SetCursor(true);
        GameManager.Instance.FindPlayer(playerHealth, cameraRoot);
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