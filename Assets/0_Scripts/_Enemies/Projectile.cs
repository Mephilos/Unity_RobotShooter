using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] GameObject projectileHitVFX;
    [SerializeField] float lifeTime = 3f;
    int damage;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // void Start()
    // {
    //     rb.linearVelocity = transform.forward * projectileSpeed;
    // }

    void OnEnable()
    {
        rb.linearVelocity = transform.forward * projectileSpeed;

        CancelInvoke(nameof(Release));
        Invoke(nameof(Release), lifeTime);
    }

    public void Initialize(int amount)
    {
        this.damage = amount;
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        playerHealth?.TakeDamage(damage);

        //Instantiate(projectileHitVFX, transform.position, Quaternion.identity);
        PoolManager.Instance.Get(projectileHitVFX, transform.position, Quaternion.identity);
        //Destroy(this.gameObject);
        Release();
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Release));
    }

    void Release()
    {
        if (!gameObject.activeSelf) return;
        PoolManager.Instance.Release(gameObject);
    }
}
