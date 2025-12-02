using UnityEngine;

public enum DamageType
{
    Normal
}
public interface IDamageable
{
    void TakeDamage(int damage, Vector3 hitPoint, DamageType type);
}
