using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] GameObject projectileHitVFX;

    float projectileSpeed;
    float lifeTime;
    int damage;
    Rigidbody rb;

    float currentLifeTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void OnEnable()
    {
        currentLifeTime = 0f;
    }

    void Update()
    {
        if (currentLifeTime < lifeTime)
        {
            currentLifeTime += Time.deltaTime;
        }
        else
        {
            Release();
        }
    }

    public void Initialize(int damage, float speed, float lifeTime)
    {
        projectileSpeed = speed;
        this.damage = damage;
        this.lifeTime = lifeTime;
        rb.linearVelocity = transform.forward * projectileSpeed;

        currentLifeTime = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        playerHealth?.TakeDamage(damage);

        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];
            Quaternion effectRotation = Quaternion.LookRotation(contact.normal);

            PoolManager.Instance.Get(projectileHitVFX, contact.point, effectRotation);
        }
        else
        {
            PoolManager.Instance.Get(projectileHitVFX, transform.position, Quaternion.identity);
        }
        Release();
    }

    void Release()
    {
        if (!gameObject.activeSelf) return;
        PoolManager.Instance.Release(gameObject);
    }
}

// using UnityEngine;

// public class Projectile : MonoBehaviour
// {
//     [SerializeField] GameObject projectileHitVFX;

//     float projectileSpeed;
//     float lifeTime;
//     int damage;
//     Rigidbody rb;

//     void Awake()
//     {
//         rb = GetComponent<Rigidbody>();
//         rb.freezeRotation = true;
//     }
//     // void Start()
//     // {
//     //     rb.linearVelocity = transform.forward * projectileSpeed;
//     // }

//     void OnEnable()
//     {
//         CancelInvoke(nameof(Release));
//         // rb.linearVelocity = transform.forward * projectileSpeed;
//         // CancelInvoke(nameof(Release));
//         // Invoke(nameof(Release), lifeTime);
//     }

//     public void Initialize(int damage, float speed, float lifeTime)
//     {
//         projectileSpeed = speed;
//         this.damage = damage;
//         this.lifeTime = lifeTime;
//         rb.linearVelocity = transform.forward * projectileSpeed;

//         CancelInvoke(nameof(Release));
//         Invoke(nameof(Release), lifeTime);
//     }

//     // void OnTriggerEnter(Collider other)
//     // {
//     //     PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
//     //     playerHealth?.TakeDamage(damage);

//     //     //Instantiate(projectileHitVFX, transform.position, Quaternion.identity);
//     //     PoolManager.Instance.Get(projectileHitVFX, transform.position, Quaternion.identity);
//     //     //Destroy(this.gameObject);
//     //     Release();
//     // }

//     private void OnCollisionEnter(Collision collision)
//     {
//         PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
//         playerHealth?.TakeDamage(damage);

//         if (collision.contacts.Length > 0)
//         {
//             ContactPoint contact = collision.contacts[0];
//             Quaternion effectRotation = Quaternion.LookRotation(contact.normal);

//             PoolManager.Instance.Get(projectileHitVFX, contact.point, effectRotation);
//         }
//         else
//         {
//             PoolManager.Instance.Get(projectileHitVFX, transform.position, Quaternion.identity);
//         }
//         Release();
//     }

//     void OnDisable()
//     {
//         CancelInvoke(nameof(Release));
//     }

//     void Release()
//     {
//         if (!gameObject.activeSelf) return;
//         PoolManager.Instance.Release(gameObject);
//     }
// }
