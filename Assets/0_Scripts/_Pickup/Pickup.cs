using UnityEngine;
using System;
public abstract class Pickup : MonoBehaviour
{
    public event Action<Pickup> OnPickup;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Constants.PLAYER_TAG))
        {
            ActiveWeapon activeWeapon = other.GetComponentInChildren<ActiveWeapon>();
            OnPick(activeWeapon);
            OnPickup?.Invoke(this);
        }
    }
    protected abstract void OnPick(ActiveWeapon AW);

    public abstract float GetRespawnTime();
}
