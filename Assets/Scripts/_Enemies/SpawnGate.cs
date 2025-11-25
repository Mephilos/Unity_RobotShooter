using System.Collections;
using UnityEngine;

public class SpawnGate : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefabs;
    [SerializeField] Transform spawnPoint;
    [SerializeField] float enemySpawnTime = 5f;
    PlayerHealth player;

    void Start()
    {
        player = FindFirstObjectByType<PlayerHealth>();
        StartCoroutine(EnemySpawnRoutine());
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
}
