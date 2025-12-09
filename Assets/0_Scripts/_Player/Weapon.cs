using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] LayerMask InteractionLayer;
    ParticleSystem muzzleFlash;
    CinemachineImpulseSource impulseSource;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    void Start()
    {
        muzzleFlash = GetComponentInChildren<ParticleSystem>();
    }
    public void Shoot(WeaponSO weaponSO, float currentSpread)
    {
        muzzleFlash.Play();
        impulseSource.GenerateImpulse();

        float randomPitch = Random.Range(.9f, 1.1f);
        SoundManager.Instance.PlaySFX(weaponSO.ShootClip, transform.position, randomPitch);

        ScoreManager.Instance.ReportShot();
        RaycastHit hit;

        // 반동 설정
        Vector3 spread = Camera.main.transform.up * Random.Range(-currentSpread, currentSpread) +
                            Camera.main.transform.right * Random.Range(-currentSpread, currentSpread);
        Vector3 shootDirection = (Camera.main.transform.forward + spread).normalized;


        if (Physics.Raycast(Camera.main.transform.position, shootDirection, out hit, Mathf.Infinity,
                             InteractionLayer, QueryTriggerInteraction.Ignore))
        {
            Quaternion effectRotation = Quaternion.LookRotation(hit.normal);
            // Instantiate(weaponSO.HitVFXPrefab, hit.point, effectRotation);

            // PoolManager.Instance.Get(weaponSO.HitVFXPrefab.gameObject, hit.point, effectRotation);

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            bool isWeak = false;

            if (hit.collider.TryGetComponent<WeakPoint>(out WeakPoint weakPoint))
            {
                PoolManager.Instance.Get(weaponSO.CriVFXPrefab.gameObject, hit.point, effectRotation);
                isWeak = true;
                damageable = weakPoint;
            }
            else
            {
                PoolManager.Instance.Get(weaponSO.HitVFXPrefab.gameObject, hit.point, effectRotation);
            }

            if (damageable != null)
            {
                ScoreManager.Instance.ReportHit();
                damageable.TakeDamage(weaponSO.Damage, hit.point, DamageType.Normal);

                HitIndicator.Instance.ShowMaker(isWeak);
            }
        }
    }
}

