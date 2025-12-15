using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] int maxEnemyCount = 5;
    [SerializeField] float spawnInterval = 3f;

    [SerializeField] float minSpawnDist = 15f;
    [SerializeField] float maxSpawnDist = 30f;
    [SerializeField] LayerMask groundLayer;

    Action OnEnemyKilledCallback;
    int currentEnemyCount = 0;
    bool isSpawning = false;

    public void StartSpawning(Action onKilledCallback)
    {
        this.OnEnemyKilledCallback = onKilledCallback;
        currentEnemyCount = 0;
        isSpawning = true;
        StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            if (currentEnemyCount < maxEnemyCount)
            {
                TrySpawnEnemy();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void TrySpawnEnemy()
    {
        if (GameManager.Instance.Player == null) return;
        Transform playerTF = GameManager.Instance.Player.transform;

        Vector3 spawnPos;

        if (GetValidSpawnPosition(playerTF, out spawnPos))
        {
            GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = PoolManager.Instance.Get(prefab, spawnPos, Quaternion.identity);

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            Action deathHandler = null;
            deathHandler = () =>
            {
                currentEnemyCount--;
                OnEnemyKilledCallback?.Invoke();
                health.OnDeath -= deathHandler;
            };
            health.OnDeath += deathHandler;

            currentEnemyCount++;
        }
    }

    bool GetValidSpawnPosition(Transform center, out Vector3 result)
    {
        for (int i = 0; i < 15; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minSpawnDist, maxSpawnDist);
            Vector3 targetPos = center.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                if (!IsVisibleFromCamera(hit.position))
                {
                    result = hit.position;
                    return true;
                }
            }
        }
        result = Vector3.zero;
        return false;
    }

    bool IsVisibleFromCamera(Vector3 pos)
    {
        if (Camera.main == null) return false;
        Vector3 cameraVision = Camera.main.WorldToViewportPoint(pos);

        return (cameraVision.x > 0 && cameraVision.x < 1 && cameraVision.y > 0 && cameraVision.y < 1 && cameraVision.z > 0);
    }
}