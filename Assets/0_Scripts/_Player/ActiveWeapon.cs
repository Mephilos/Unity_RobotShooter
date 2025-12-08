using UnityEngine;
using StarterAssets;
using System.Collections;
using Unity.Cinemachine;
using TMPro;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO startingWeaponOS;
    [SerializeField] GameObject Zoom;
    [SerializeField] TMP_Text ammoText;
    [SerializeField] CinemachineCamera cinemachineVirtualCamera;
    [SerializeField] Camera weaponCamera;
    [SerializeField] float zoomTransSpeed = 20f;
    Weapon currentWeapon;
    Animator animator;
    StarterAssetsInputs starterAssetsInputs;
    FirstPersonController firstPersonController;
    WeaponSO weaponSO;
    WaitForSeconds waitFire;
    int currentAmmo = 0;
    float defaultFOV = 75f;
    float defaultRotationSpeed;
    float keepFireSpread = 0f;
    bool isFire = false;
    bool isZoom = false;

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
        HandleSpreadRecovery();
    }
    void HandleSpreadRecovery()
    {
        if (!isFire && keepFireSpread > 0)
        {
            keepFireSpread -= weaponSO.RecoverySpreadSpeed * Time.deltaTime;
            if (keepFireSpread < 0) keepFireSpread = 0;
        }
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
            PoolManager.Instance.Release(currentWeapon.gameObject);
        }

        // Weapon newWeapon = Instantiate(weaponSO.WeaponPrefab, transform).GetComponent<Weapon>();

        GameObject newWeaponObj = PoolManager.Instance.Get(weaponSO.WeaponPrefab, transform.position, transform.rotation);
        newWeaponObj.transform.SetParent(transform);
        newWeaponObj.transform.localPosition = weaponSO.WeaponPrefab.transform.position;
        newWeaponObj.transform.localRotation = weaponSO.WeaponPrefab.transform.rotation;

        Weapon newWeapon = newWeaponObj.GetComponent<Weapon>();
        currentWeapon = newWeapon;
        this.weaponSO = weaponSO;

        waitFire = new WaitForSeconds(weaponSO.FireRate);

        currentAmmo = 0;
        keepFireSpread = 0;
        AdjustAmmo(weaponSO.MagazineSize);
    }

    void HandleShoot()
    {
        if (!starterAssetsInputs.shoot || isFire || currentAmmo <= 0) return;

        isFire = true;

        animator.Play(Constants.ANIMATION_NAME, 0, 0);

        float currentSpread = GetCurrentSpread();

        currentWeapon.Shoot(weaponSO, currentSpread);
        if (!isZoom)
        {
            firstPersonController.ApplyRecoil(weaponSO.RecoilForce * Time.deltaTime * 50f);
        }

        keepFireSpread += weaponSO.IncreaseSpreadPerShot;

        AdjustAmmo(-1);

        if (!weaponSO.isAutomatic)
        {
            starterAssetsInputs.ShootInput(false);
        }
        StartCoroutine(FireRateRoutine());
    }

    IEnumerator FireRateRoutine()
    {
        yield return waitFire;
        isFire = false;
    }

    void HandleZoom()
    {
        if (!weaponSO.CanZoom) return;

        if (starterAssetsInputs.zoom != isZoom)
        {
            Debug.Log("줌 변환");
            isZoom = starterAssetsInputs.zoom;

            Zoom.SetActive(isZoom);

            if (isZoom) firstPersonController.ChangeRotationSpeed(weaponSO.ZoomSpeed); // 줌인 감도
            else firstPersonController.ChangeRotationSpeed(defaultRotationSpeed); // 디폴트 감도
        }
        float changeFOV = isZoom ? weaponSO.ZoomAmount : defaultFOV;

        if (Mathf.Abs(cinemachineVirtualCamera.Lens.FieldOfView - changeFOV) > 0.1f)
        {
            float newFOV = Mathf.Lerp(cinemachineVirtualCamera.Lens.FieldOfView, changeFOV, Time.deltaTime * zoomTransSpeed);

            cinemachineVirtualCamera.Lens.FieldOfView = newFOV;
            weaponCamera.fieldOfView = newFOV;
        }
    }

    public float GetCurrentSpread()
    {
        float currentSpread = weaponSO.DefaultSpread;
        float currentSpeed = firstPersonController.GetCurrentSpeed();

        if (currentSpeed > 0.1f)
        {
            currentSpread += weaponSO.MoveSpreadFactor * (currentSpeed / firstPersonController.SprintSpeed);
        }

        currentSpread += keepFireSpread;

        if (isZoom) currentSpread *= 0.1f;

        return Mathf.Min(currentSpread, weaponSO.MaxSpread);
    }
}
