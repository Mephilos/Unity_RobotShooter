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
    public void Shoot(WeaponSO weaponSO)
    {
        muzzleFlash.Play();
        impulseSource.GenerateImpulse();

        ScoreManager.Instance.ReportShot();
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity,
                             InteractionLayer, QueryTriggerInteraction.Ignore))
        {
            Quaternion effectRotation = Quaternion.LookRotation(hit.normal);
            // Instantiate(weaponSO.HitVFXPrefab, hit.point, effectRotation);
            PoolManager.Instance.Get(weaponSO.HitVFXPrefab.gameObject, hit.point, effectRotation);

            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                ScoreManager.Instance.ReportHit();
                damageable.TakeDamage(weaponSO.Damage, hit.point, DamageType.Normal);
            }
        }
    }
}

