using System.Collections;
using NUnit.Framework.Internal;
using Unity.Mathematics;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] Transform turretHead;
    [SerializeField] Transform projectileFirePoint;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float fireInterval = 3f;
    [SerializeField] int damage = 2;
    ActiveWeapon playerTargetPoint;
    void Start()
    {
        playerTargetPoint = FindAnyObjectByType<ActiveWeapon>();
        StartCoroutine(FireProjectile());
    }
    void Update()
    {
        turretHead.LookAt(playerTargetPoint.transform);
    }

    IEnumerator FireProjectile()
    {
        while (playerTargetPoint)
        {
            yield return new WaitForSeconds(fireInterval);

            Projectile newProjectile = Instantiate(projectilePrefab, projectileFirePoint.position, turretHead.rotation).GetComponent<Projectile>();
            newProjectile.transform.LookAt(playerTargetPoint.transform);
            newProjectile.Initialize(damage);
        }
    }
}
