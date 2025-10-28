using UnityEngine;
using StarterAssets;
using System.Collections;
using Cinemachine;
using TMPro;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO startingWeaponOS;
    [SerializeField] GameObject Zoom;
    [SerializeField] TMP_Text ammoText;
    [SerializeField] CinemachineVirtualCamera cinemachineVirtualCamera;
    [SerializeField] Camera weaponCamera;
    Weapon currentWeapon;
    Animator animator;
    StarterAssetsInputs starterAssetsInputs;
    FirstPersonController firstPersonController;
    WeaponSO weaponSO;

    int currentAmmo = 0;
    float defaultFOV = 75f;
    float defaultRotationSpeed;
    bool isFire = false;

    void Awake()
    {
        firstPersonController = GetComponentInParent<FirstPersonController>();
        animator = GetComponentInParent<Animator>();
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        defaultRotationSpeed = firstPersonController.RotationSpeed;
    }
    void Start()
    {
        SwitchWeapon(startingWeaponOS);
    }
    void Update()
    {
        HandleShoot();
        HandleZoom();
    }

    public void AdjustAmmo(int Amount)
    {
        currentAmmo += Amount;
        if (currentAmmo >= weaponSO.MagazineSize)
        {
            currentAmmo = weaponSO.MagazineSize;
        }
        ammoText.text = currentAmmo.ToString("D2");
    }

    public void SwitchWeapon(WeaponSO weaponSO)
    {
        if (currentWeapon != null)
        {
            Destroy(currentWeapon.gameObject);
        }

        Weapon newWeapon = Instantiate(weaponSO.WeaponPrefab, transform).GetComponent<Weapon>();
        currentWeapon = newWeapon;
        this.weaponSO = weaponSO;
        currentAmmo = 0;
        AdjustAmmo(weaponSO.MagazineSize);
    }
    void HandleShoot()
    {
        if (!starterAssetsInputs.shoot || isFire || currentAmmo <= 0) return;

        isFire = true;

        animator.Play(Constants.ANIMATION_NAME, 0, 0);

        currentWeapon.Shoot(weaponSO);
        AdjustAmmo(-1);

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

    void HandleZoom()
    {
        if (!weaponSO.CanZoom || !cinemachineVirtualCamera) return;

        if (!starterAssetsInputs.zoom)
        {
            Zoom.SetActive(false);
            cinemachineVirtualCamera.m_Lens.FieldOfView = defaultFOV;
            weaponCamera.fieldOfView = defaultFOV;
            firstPersonController.ChangeRotationSpeed(defaultRotationSpeed);
        }

        else
        {
            Zoom.SetActive(true);
            cinemachineVirtualCamera.m_Lens.FieldOfView = weaponSO.ZoomAmount;
            weaponCamera.fieldOfView = weaponSO.ZoomAmount;
            firstPersonController.ChangeRotationSpeed(weaponSO.ZoomSpeed);
        }
    }
}
