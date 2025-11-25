using UnityEngine;

public class AmmoPickup : Pickup
{
    [SerializeField] AmmoSO ammoSO;

    protected override void OnPick(ActiveWeapon AW)
    {
        AW.AdjustAmmo(ammoSO.AmmoValue);
    }

    public override float GetRespawnTime()
    {
        return ammoSO != null ? ammoSO.RespawnTime : 30f;
    }
}
