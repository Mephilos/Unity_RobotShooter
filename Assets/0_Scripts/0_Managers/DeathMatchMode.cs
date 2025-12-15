using UnityEngine;
using System.Collections;
public class DeathMatchMode : MonoBehaviour
{
    [SerializeField] int targetKillScore = 20;
    [SerializeField] PlayerSpawner playerSpawner;
    [SerializeField] EnemySpawner enemySpawner;

    int playerScore = 0;
    int enemyScore = 0;
    bool isGameEnded = false;

    void Start()
    {
        playerScore = 0;
        enemyScore = 0;
        isGameEnded = false;
        playerSpawner.SpawnPlayer();
        playerSpawner.OnPlayerDeath += HandlePlayerDeath;
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        while (GameManager.Instance.Player == null) yield return null;
        enemySpawner.StartSpawning(HandleEnemyDeath);
    }
    void HandlePlayerDeath()
    {
        if (isGameEnded) return;

        enemyScore++;

        if (enemyScore >= targetKillScore)
        {
            EndGame(false);
        }
        else
        {
            playerSpawner.RequestRespawn();
        }
    }

    void HandleEnemyDeath()
    {
        if (isGameEnded) return;

        playerScore++;

        if (playerScore >= targetKillScore)
        {
            EndGame(true);
        }
    }

    void EndGame(bool playerWin)
    {
        isGameEnded = true;
        enemySpawner.StopSpawning();
        playerSpawner.OnPlayerDeath -= HandlePlayerDeath;

        // 임시
        if (playerWin)
        {
            LevelManager.Instance.AdjustEnemiesLeft(-9999);
        }
        else
        {
        }
    }
}
