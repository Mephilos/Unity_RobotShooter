using UnityEngine;
using StarterAssets;
using System.Collections;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    Animator animator;
    StarterAssetsInputs starterAssetsInputs;
    Weapon currentWeapon;

    bool isFire = false;

    void Awake()
    {
        animator = GetComponentInParent<Animator>();
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
    }
    void Start()
    {
        currentWeapon = GetComponentInChildren<Weapon>();
    }
    void Update()
    {
        HandleShoot();
    }

    public void SwitchWeapon(WeaponSO weaponSO)
    {
        if (currentWeapon)
        {
            Destroy(currentWeapon.gameObject);
        }

        Weapon newWeapon = Instantiate(weaponSO.WeaponPrefab, transform).GetComponent<Weapon>();
        currentWeapon = newWeapon;
        this.weaponSO = weaponSO;
    }
    void HandleShoot()
    {
        if (!starterAssetsInputs.shoot || isFire) return;

        isFire = true;

        animator.Play(Constants.ANIMATION_NAME, 0, 0);

        currentWeapon.Shoot(weaponSO);
        if (!weaponSO.isAutomatic)
        {
            starterAssetsInputs.ShootInput(false);
        }
        StartCoroutine(FireRateRoutine());
    }

    IEnumerator FireRateRoutine()
    {
        yield return new WaitForSeconds(weaponSO.FireRate);
        isFire = false;
    }
}
