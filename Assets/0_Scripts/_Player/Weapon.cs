using Unity.Cinemachine;
using UnityEngine;
using MoreMountains.Feedbacks;

public class Weapon : MonoBehaviour
{
    [SerializeField] MMF_Player shootFeedback;
    [SerializeField] LayerMask InteractionLayer;
    [SerializeField] ParticleSystem muzzleFlash;
    CinemachineImpulseSource impulseSource;
    Camera mainCamera;
    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        mainCamera = Camera.main;
    }

    public void Shoot(WeaponSO weaponSO, float currentSpeed)
    {
        if (mainCamera == null) return;
        PlayShootEffect(weaponSO);
        ScoreManager.Instance.ReportShot();
        Vector3 shootDirection = ApplyShootSpread(currentSpeed);

        if (Physics.Raycast(mainCamera.transform.position, shootDirection, out RaycastHit hit, Mathf.Infinity,
                     InteractionLayer, QueryTriggerInteraction.Ignore))
        {
            HandleShootHit(hit, weaponSO);
        }

    }

    public void HandleShootHit(RaycastHit hit, WeaponSO weaponSO)
    {
        Quaternion effectRotation = Quaternion.LookRotation(hit.normal);
        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
        GameObject vfxPrefab = weaponSO.HitVFXPrefab.gameObject;
        bool isWeak = false;

        if (hit.collider.TryGetComponent<WeakPoint>(out WeakPoint weakPoint))
        {
            vfxPrefab = weaponSO.CriVFXPrefab.gameObject;
            isWeak = true;
            damageable = weakPoint;
        }

        PoolManager.Instance.Get(vfxPrefab, hit.point, effectRotation);

        if (damageable != null)
        {
            damageable.TakeDamage(weaponSO.Damage, hit.point, DamageType.Normal);

            ScoreManager.Instance.ReportHit();

            HitIndicator.Instance.ShowMaker(isWeak);
        }

    }

    Vector3 ApplyShootSpread(float currentSpread)
    {
        Vector3 spread = mainCamera.transform.up * Random.Range(-currentSpread, currentSpread) +
                            mainCamera.transform.right * Random.Range(-currentSpread, currentSpread);
        Vector3 shootDirection = (mainCamera.transform.forward + spread).normalized;

        return shootDirection;
    }

    void PlayShootEffect(WeaponSO weaponSO)
    {
        muzzleFlash.Play();
        impulseSource.GenerateImpulse();
        float randomPitch = Random.Range(.9f, 1.1f);
        SoundManager.Instance.PlaySFX(weaponSO.ShootClip, transform.position, randomPitch);
        shootFeedback?.PlayFeedbacks();
    }
}

