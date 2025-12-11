using System.Collections;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    [SerializeField] EnemyWeaponSO weaponSO;
    [SerializeField] Transform firePoint;

    public float FireRate => weaponSO.FireRate;

    public IEnumerator FireBurst(Vector3 targetPosition)
    {
        for (int i = 0; i < weaponSO.BurstCount; i++)
        {
            FireProjectile(targetPosition);
            // 마지막 발사는 코루틴 작동x;
            if (i < weaponSO.BurstCount - 1)
            {
                yield return new WaitForSeconds(weaponSO.BurstInterval);
            }
        }
    }

    void FireProjectile(Vector3 playerPosition)
    {
        Vector3 spawnPosition = firePoint.position;
        Vector3 direction = (playerPosition - spawnPosition).normalized;

        float accErr = weaponSO.AccuracyError;

        direction.x += Random.Range(-accErr, accErr) * .01f;
        direction.y += Random.Range(-accErr, accErr) * .01f;
        direction.z += Random.Range(-accErr, accErr) * .01f;

        GameObject newProjectile = PoolManager.Instance.Get(weaponSO.ProjectilePrefab, spawnPosition, Quaternion.LookRotation(direction));
        if (newProjectile.TryGetComponent<Projectile>(out Projectile p))
        {
            p.Initialize(weaponSO.Damage, weaponSO.ProjectileSpeed, weaponSO.ProjectileLifeTime);
        }
        Debug.Log("적발사");
    }
}
