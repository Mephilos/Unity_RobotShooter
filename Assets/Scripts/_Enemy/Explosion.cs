using System;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] float radius = 2f;
    [SerializeField] int explosionDamage = 5;
    void Start()
    {
        Explode();
    }

    private void Explode()
    {
        Collider[] hitCol = Physics.OverlapSphere(transform.position, 2f);

        foreach (Collider hit in hitCol)
        {
            if (hit.CompareTag(Constants.PLAYER_TAG))
            {
                if (hit.TryGetComponent<PlayerHealth>(out PlayerHealth playerHealth))
                {
                    playerHealth.TakeDamage(explosionDamage);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
