using Cinemachine;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera deathVirtualCam;
    [SerializeField] Transform weaponCamera;
    [SerializeField] int startingHealth = 3;
    int currentHitPoint;
    int gameOverVirtualCameraPrioity = 20;
    void Awake()
    {
        currentHitPoint = startingHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHitPoint -= amount;

        if (currentHitPoint <= 0)
        {
            weaponCamera.parent = null;
            deathVirtualCam.Priority = gameOverVirtualCameraPrioity;
            Destroy(this.gameObject);
        }
    }
}
