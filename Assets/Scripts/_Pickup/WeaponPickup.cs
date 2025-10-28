using UnityEngine;

public class WeaponPickup : Pickup
{
    [SerializeField] WeaponSO weaponSO;

    protected override void OnPick(ActiveWeapon AW)
    {
        AW.SwitchWeapon(weaponSO);
    }
}
