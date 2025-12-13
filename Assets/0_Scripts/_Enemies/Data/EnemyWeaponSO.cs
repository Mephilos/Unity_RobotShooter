using UnityEngine;

public enum EnemyWeaponType
{
    Burst,
    ShotGun,
    Sniper
}

[CreateAssetMenu(fileName = "NewEnemyWeaponData", menuName = "Enemy/WeaponData")]
public class EnemyWeaponSO : ScriptableObject
{
    public EnemyWeaponType weaponType = EnemyWeaponType.Burst;
    public GameObject ProjectilePrefab;
    public GameObject MuzzleFlashVFX;
    public float ProjectileSpeed = 20f;
    public float ProjectileLifeTime = 3f;

    public int Damage = 10;
    public float FireRate = 0.5f;
    public float AttackRange = 15f;
    [Tooltip("탄의 퍼점정도")]
    public float AccuracyError = 2.0f;

    public int BurstCount = 3;
    public float BurstInterval = 0.1f;


    [Range(0.0f, 1.0f)]
    public float ShootingMoveSpeedPenalty = 0.5f;
    [Range(0.0f, 1.0f)]
    public float CombatStateMoveSpeedPenalty = 0.8f;
}
