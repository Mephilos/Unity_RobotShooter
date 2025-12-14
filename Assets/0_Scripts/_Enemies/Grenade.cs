using UnityEngine;

public class Grenade : MonoBehaviour
{
    [SerializeField] GameObject explosionEffect;

    Rigidbody rb;
    bool isExplod = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Throw(Vector3 velocity)
    {
        rb.linearVelocity = velocity;
        rb.angularVelocity = new Vector3(Random.Range(-10f, -10f), Random.Range(-10f, -10f), Random.Range(-10f, -10f));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isExplod) return;

        Explode();
        // CancelInvoke(nameof(Explode));
        // Invoke(nameof(Explode), 3f);
    }

    void Explode()
    {
        if (isExplod) return;
        isExplod = true;

        PoolManager.Instance.Get(explosionEffect, transform.position, Quaternion.identity);
        PoolManager.Instance.Release(gameObject);
    }

    void OnDisable()
    {
        isExplod = false;
        // CancelInvoke();
    }
}
