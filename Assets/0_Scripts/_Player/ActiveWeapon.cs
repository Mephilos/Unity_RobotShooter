using UnityEngine;
using StarterAssets;
using System.Collections;
using Unity.Cinemachine;
using System;

public class ActiveWeapon : MonoBehaviour
{
    [SerializeField] WeaponSO startingWeaponOS;
    [SerializeField] GameObject Zoom;
    // [SerializeField] TMP_Text ammoText;
    [SerializeField] CinemachineCamera cinemachineVirtualCamera;
    [SerializeField] Camera weaponCamera;
    [SerializeField] float zoomTransSpeed = 20f;
    [SerializeField] bool isInfinityAmmo;

    public event Action<int, int> OnAmmoChange;
    public float DefaultRotationSpeed => defaultRotationSpeed;
    Weapon currentWeapon;
    // Animator animator;
    StarterAssetsInputs starterAssetsInputs;
    FirstPersonController firstPersonController;
    WeaponSO weaponSO;
    WaitForSeconds waitFire;
    int currentAmmo = 0;
    float defaultFOV = 75f;
    float defaultRotationSpeed;
    float keepFireRecoilPenalty = 0f;
    bool isFire = false;
    bool isZoom = false;

    void Awake()
    {
        firstPersonController = GetComponentInParent<FirstPersonController>();
        // animator = GetComponentInParent<Animator>();
        starterAssetsInputs = GetComponentInParent<StarterAssetsInputs>();
        // defaultRotationSpeed = firstPersonController.RotationSpeed;
    }

    void Start()
    {
        defaultRotationSpeed = PlayerPrefs.GetFloat("MouseSens", 2.0f);
        SwitchWeapon(startingWeaponOS);
    }

    void Update()
    {
        HandleShoot();
        HandleZoom();
        HandleSpreadRecovery();
    }

    public void AdjustAmmo(int Amount)
    {
        currentAmmo += Amount;
        if (currentAmmo >= weaponSO.MagazineSize)
        {
            currentAmmo = weaponSO.MagazineSize;
        }
        OnAmmoChange?.Invoke(currentAmmo, weaponSO.MagazineSize);
        //ammoText.text = currentAmmo.ToString("D2");
    }
    public (int currentAmmo, int maxAmmo) GetAmmo()
    {
        if (weaponSO == null) return (0, 0);
        int currentAmmo = this.currentAmmo;
        int maxAmmo = weaponSO.MagazineSize;
        return (currentAmmo, maxAmmo);
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
        keepFireRecoilPenalty = 0;
        AdjustAmmo(weaponSO.MagazineSize);
    }

    void HandleShoot()
    {
        if (GameManager.Instance.IsPause) return;
        if (!starterAssetsInputs.shoot || isFire || currentAmmo <= 0) return;

        isFire = true;

        //animator.Play(Constants.ANIMATION_NAME, 0, 0);

        currentWeapon.Shoot(weaponSO, GetCurrentSpread());

        if (!isZoom)
        {
            float currentRecoil = Mathf.Min(weaponSO.DefaultRecoil +
                                            (Mathf.Pow(keepFireRecoilPenalty, 2) * weaponSO.RecoilFactor), weaponSO.MaxRecoil);
            firstPersonController.ApplyRecoil(currentRecoil * Time.deltaTime * 50f);
        }

        keepFireRecoilPenalty += 1f;

        if (!isInfinityAmmo)
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
        if (GameManager.Instance.IsPause) return;
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

    void HandleSpreadRecovery()
    {
        if (!isFire && keepFireRecoilPenalty > 0)
        {
            keepFireRecoilPenalty = Mathf.Lerp(keepFireRecoilPenalty, 0f, weaponSO.RecoverySpreadSpeed * Time.deltaTime);
            if (keepFireRecoilPenalty < 0) keepFireRecoilPenalty = 0;
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
        float keepFirePenalty = Mathf.Pow(keepFireRecoilPenalty, 2) * weaponSO.IncreaseSpreadPerShot;
        currentSpread += keepFirePenalty;

        if (isZoom) currentSpread *= 0.1f;

        return Mathf.Min(currentSpread, weaponSO.MaxSpread);
    }

    public void UpdateSensitivity(float newSensitivity)
    {
        defaultRotationSpeed = newSensitivity;

        if (!isZoom)
        {
            firstPersonController.ChangeRotationSpeed(defaultRotationSpeed);
        }
    }

    public void SetupReferences(CinemachineCamera playerCam, GameObject zoomUI)
    {
        cinemachineVirtualCamera = playerCam;
        Zoom = zoomUI;
    }
}
