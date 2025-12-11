using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyWeaponData", menuName = "Enemy/WeaponData")]
public class EnemyWeaponSO : ScriptableObject
{
    public GameObject ProjectilePrefab;
    public float ProjectileSpeed = 20f;
    public float ProjectileLifeTime = 3f;

    public int Damage = 10;
    public float FireRate = 0.5f;
    public float AttackRange = 15f;
    public float AccuracyError = 2.0f;
    public int BurstCount = 3;
}
