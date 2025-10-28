using UnityEngine;

public class AmmoPickup : Pickup
{
    [SerializeField] int ammoValue = 10;
    protected override void OnPick(ActiveWeapon AW)
    {
        AW.AdjustAmmo(ammoValue);
    }
}
