using UnityEngine;
using StarterAssets;
using System.Collections;
using Cinemachine;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO weaponSO;
    [SerializeField] GameObject Zoom;
    Animator animator;
    StarterAssetsInputs starterAssetsInputs;
    CinemachineVirtualCamera cinemachineVirtualCamera;
    FirstPersonController firstPersonController;

    Weapon currentWeapon;
    float defaultFOV = 75f;
    bool isFire = false;

    void Awake()
    {
        firstPersonController = GetComponentInParent<FirstPersonController>();
        animator = GetComponentInParent<Animator>();
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        cinemachineVirtualCamera = FindFirstObjectByType<CinemachineVirtualCamera>();
    }
    void Start()
    {
        currentWeapon = GetComponentInChildren<Weapon>();
    }
    void Update()
    {
        HandleShoot();
        HandleZoom();
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

    void HandleZoom()
    {
        if (!weaponSO.CanZoom || !cinemachineVirtualCamera) return;

        if (!starterAssetsInputs.zoom)
        {
            Zoom.SetActive(false);
            cinemachineVirtualCamera.m_Lens.FieldOfView = defaultFOV;
        }

        else
        {
            Zoom.SetActive(true);
            cinemachineVirtualCamera.m_Lens.FieldOfView = weaponSO.ZoomAmount;
        }
    }
}
