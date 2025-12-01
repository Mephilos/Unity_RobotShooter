using System;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] float radius = 2f;
    [SerializeField] int explosionDamage = 5;

    Collider[] hitCol = new Collider[10];
    void OnEnable()
    {
        Explode();
    }

    private void Explode()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, hitCol);
        //Collider[] hitCol = Physics.OverlapSphere(transform.position, 2f);

        for (int i = 0; i < count; i++)
        {
            Collider hit = hitCol[i];
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
