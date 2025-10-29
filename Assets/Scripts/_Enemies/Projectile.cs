using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 10f;
    int damage;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        rb.linearVelocity = transform.forward * projectileSpeed;
    }

    public void Initialize(int amount)
    {
        this.damage = amount;
    }
    void OnTriggerEnter(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        playerHealth?.TakeDamage(damage);

        Destroy(this.gameObject);
    }
}
