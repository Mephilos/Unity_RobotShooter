using System.Collections;

using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    [SerializeField] EnemyWeaponSO weaponSO;
    [SerializeField] Transform firePoint;
    [SerializeField] LayerMask blockLayer;

    EnemySight enemySight;
    public float FireRate => weaponSO.FireRate;
    public float WeaponRange => weaponSO.AttackRange;

    public float ShootingPenalty => weaponSO.ShootingMoveSpeedPenalty;
    public float CombatStatePenalty => weaponSO.CombatStateMoveSpeedPenalty;

    void Awake()
    {
        enemySight = GetComponent<EnemySight>();
    }

    public IEnumerator FireBurst(Vector3 playerPosition)
    {
        if (weaponSO.weaponType == EnemyWeaponType.ShotGun)
        {
            if (ShootLineCheck())
            {
                for (int i = 0; i < weaponSO.BurstCount; i++)
                {
                    FireProjectile(playerPosition);
                }
            }
        }
        else
        {
            for (int i = 0; i < weaponSO.BurstCount; i++)
            {
                if (ShootLineCheck())
                {
                    FireProjectile(playerPosition);
                }
                // 마지막 발사는 코루틴 작동x;
                if (i < weaponSO.BurstCount - 1)
                {
                    yield return new WaitForSeconds(weaponSO.BurstInterval);
                }

            }
        }
    }

    bool ShootLineCheck()
    {
        Vector3 eyePosition = enemySight.EyePosition;
        Vector3 muzzlePosition = firePoint.position;

        if (Physics.Linecast(eyePosition, muzzlePosition, blockLayer))
        {
            return false;
        }
        return true;
    }

    void FireProjectile(Vector3 playerPosition)
    {
        Vector3 spawnPosition = firePoint.position;
        Vector3 direction = (playerPosition - spawnPosition).normalized;

        float accErr = weaponSO.AccuracyError;

        direction.x += Random.Range(-accErr, accErr) * .01f;
        direction.y += Random.Range(-accErr, accErr) * .01f;
        direction.z += Random.Range(-accErr, accErr) * .01f;

        PoolManager.Instance.Get(weaponSO.MuzzleFlashVFX, spawnPosition, firePoint.rotation);

        GameObject newProjectile = PoolManager.Instance.Get(weaponSO.ProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        if (newProjectile.TryGetComponent<Projectile>(out Projectile p))
        {
            p.Initialize(weaponSO.Damage, weaponSO.ProjectileSpeed, weaponSO.ProjectileLifeTime);
        }
    }
}
