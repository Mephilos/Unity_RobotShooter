using UnityEngine;

[CreateAssetMenu(fileName = "WeaponSO", menuName = "Scriptable Objects/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public GameObject WeaponPrefab;
    public int Damage = 1;
    public float FireRate = 100f;
    public ParticleSystem HitVFXPrefab;
    public ParticleSystem CriVFXPrefab;
    public bool isAutomatic = false;
    public int MagazineSize = 10;
    public float RespawnTime = 15f;

    public bool CanZoom = false;
    public float ZoomAmount = 10f;
    public float ZoomSpeed = .5f;

    public float DefaultSpread = 0.1f;
    public float IncreaseSpreadPerShot = 0.002f;
    public float MaxSpread = 0.2f;
    public float MoveSpreadFactor = 0.5f;
    public float DefaultRecoil = 1.0f;
    public float RecoilFactor = 0.02f;
    public float MaxRecoil = 10f;
    public float RecoverySpreadSpeed = 8.0f;

    public AudioClip ShootClip;
}
