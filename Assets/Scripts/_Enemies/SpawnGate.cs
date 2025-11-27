using Unity.Mathematics;
using System.Collections;
using UnityEngine;

public class SpawnGate : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefabs;
    [SerializeField] GameObject deathParticle;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float enemySpawnTime = 5f;
    PlayerHealth player;
    EnemyHealth enemyHealth;
    void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }
    void OnEnable()
    {
        enemyHealth.OnDeath += Death;
    }
    void Start()
    {
        player = FindFirstObjectByType<PlayerHealth>();
        StartCoroutine(EnemySpawnRoutine());
    }

    void OnDisable()
    {
        enemyHealth.OnDeath -= Death;
    }
    IEnumerator EnemySpawnRoutine()
    {
        while (true)
        {
            if (!player)
            {
                yield break;
            }

            Instantiate(enemyPrefabs, spawnPoint.position, spawnPoint.rotation);

            yield return new WaitForSeconds(enemySpawnTime);
        }
    }
    void Death()
    {
        Instantiate(deathParticle, transform.position, quaternion.identity);
        Destroy(gameObject);
    }
}
