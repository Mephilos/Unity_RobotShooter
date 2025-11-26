using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
public class PickupSpawner : MonoBehaviour
{
    [SerializeField] Pickup pickupPrefab;
    [SerializeField] Transform spawnPoint;
    [SerializeField] Image respawnCoolTimeGaugeImg;
    [SerializeField] float rotationSpeed = 100f;

    Pickup currentPickup;
    float respawnTime;
    void Start()
    {
        respawnCoolTimeGaugeImg.gameObject.SetActive(false);
        RespawnItem();
    }

    void Update()
    {
        if (currentPickup != null)
            spawnPoint.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    void RespawnItem()
    {
        if (currentPickup == null)
        {
            currentPickup = Instantiate(pickupPrefab, spawnPoint.position, Quaternion.identity, spawnPoint);
        }
        if (currentPickup != null)
        {
            respawnTime = currentPickup.GetRespawnTime();
            currentPickup.OnPickup -= doPickup;
            currentPickup.OnPickup += doPickup;
            currentPickup.gameObject.SetActive(true);
            respawnCoolTimeGaugeImg.gameObject.SetActive(false);
        }
    }
    void doPickup(Pickup pickup)
    {
        pickup.gameObject.SetActive(false);
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        respawnCoolTimeGaugeImg.gameObject.SetActive(true);
        respawnCoolTimeGaugeImg.fillAmount = 0f;

        float timer = 0f;
        while (timer < respawnTime)
        {
            timer += Time.deltaTime;

            respawnCoolTimeGaugeImg.fillAmount = timer / respawnTime;
            yield return null;
        }
        RespawnItem();
    }
}
