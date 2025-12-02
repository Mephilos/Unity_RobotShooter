using UnityEngine;

public class WeaponPickup : Pickup
{
    [SerializeField] WeaponSO weaponSO;

    protected override void OnPick(ActiveWeapon AW)
    {
        AW.SwitchWeapon(weaponSO);
    }

    public override float GetRespawnTime()
    {
        return weaponSO != null ? weaponSO.RespawnTime : 30f;
    }
}
