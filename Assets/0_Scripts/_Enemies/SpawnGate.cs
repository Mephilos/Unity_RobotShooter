using Unity.Mathematics;
using System.Collections;
using UnityEngine;

public class SpawnGate : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefabs;
    [SerializeField] GameObject deathParticle;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float enemySpawnTime = 5f;

    EnemyHealth enemyHealth;
    WaitForSeconds wait;
    // Coroutine currentRoutine;

    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();

        wait = new WaitForSeconds(enemySpawnTime);
    }

    void OnEnable()
    {
        enemyHealth.OnDeath += Death;
    }

    void Start()
    {
        StartCoroutine(EnemySpawnRoutine());
    }

    void OnDisable()
    {
        enemyHealth.OnDeath -= Death;
    }

    IEnumerator EnemySpawnRoutine()
    {
        while (GameManager.Instance.Player == null)
        {
            yield return null;
        }

        while (true)
        {
            if (GameManager.Instance.Player != null)
            {
                //Instantiate(enemyPrefabs, spawnPoint.position, spawnPoint.rotation);
                PoolManager.Instance.Get(enemyPrefabs, spawnPoint.position, spawnPoint.rotation);
            }
            yield return wait;
        }
    }
    void Death()
    {
        Instantiate(deathParticle, transform.position, quaternion.identity);
        Destroy(gameObject);
    }
}
