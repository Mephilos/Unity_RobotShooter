using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PickupSpawner : MonoBehaviour
{
    [SerializeField] Pickup pickupPrefab;
    [SerializeField] Transform spawnPoint;

    [SerializeField] float rotationSpeed = 100f;

    Pickup currentPickup;
    float respawnTime;
    void Start()
    {
        RespawnItem();
    }

    void Update()
    {
        if (currentPickup != null)
            spawnPoint.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    void RespawnItem()
    {
        if (currentPickup != null)
        {
            currentPickup = Instantiate(pickupPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
            respawnTime = currentPickup.GetRespawnTime();
            currentPickup.OnPickup += doPickup;
        }
        else
        {
            currentPickup.gameObject.SetActive(true);
        }
    }
    void doPickup(Pickup pickup)
    {
        pickup.gameObject.SetActive(false);

        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        float respawnTimer = 0f;
        while (respawnTimer < respawnTime)
        {
            respawnTimer += Time.deltaTime;
        }
        yield return null;

        RespawnItem();
    }
}
