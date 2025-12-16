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

    int currentEnemyCount = 0;
    bool isSpawning = false;

    Action OnEnemyKilledCallback;
    WaitForSeconds spawnCoolTime;

    void Awake()
    {
        spawnCoolTime = new WaitForSeconds(spawnInterval);
    }

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
            yield return spawnCoolTime;
        }
    }

    void TrySpawnEnemy()
    {
        if (GameManager.Instance.Player == null) return;
        Transform playerTransform = GameManager.Instance.Player.transform;

        Vector3 enemySpawnPosition;

        if (GetValidSpawnPosition(playerTransform, out enemySpawnPosition))
        {
            GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = PoolManager.Instance.Get(prefab, enemySpawnPosition, Quaternion.identity);

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
            // 네임스페이스 모호함으로 인한 random 함수 네임스페이스 명시 후... 코드가 뭔가 샤프하지 못하다 짜증...
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(minSpawnDist, maxSpawnDist);

            Vector3 targetPosition = center.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                if (!IsVisibleCamera(hit.position))
                {
                    result = hit.position;
                    return true;
                }
            }
        }
        result = Vector3.zero;
        return false;
    }

    bool IsVisibleCamera(Vector3 pos)
    {
        if (Camera.main == null) return false;
        Vector3 cameraVision = Camera.main.WorldToViewportPoint(pos);

        return (cameraVision.x > 0 && cameraVision.x < 1 && cameraVision.y > 0 && cameraVision.y < 1 && cameraVision.z > 0);
    }
}