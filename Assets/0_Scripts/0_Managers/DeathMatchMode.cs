using UnityEngine;
using System.Collections;
public class DeathMatchMode : MonoBehaviour
{
    [SerializeField] int targetKillScore = 20;
    [SerializeField] PlayerSpawner playerSpawner;
    [SerializeField] EnemySpawner enemySpawner;
    [SerializeField] PlaySceneUI playSceneUI;

    int playerScore = 0;
    int enemyScore = 0;
    bool isGameEnded = false;

    void Start()
    {
        playerScore = 0;
        enemyScore = 0;
        isGameEnded = false;

        UpdateUIScore();

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
        UpdateUIScore();

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
        UpdateUIScore();

        if (playerScore >= targetKillScore)
        {
            EndGame(true);
        }
    }

    void UpdateUIScore()
    {
        if (playSceneUI != null)
        {
            playSceneUI.UpdateDeathMatchScore(playerScore, enemyScore, targetKillScore);
        }
    }

    void EndGame(bool playerWin)
    {
        isGameEnded = true;
        enemySpawner.StopSpawning();
        playerSpawner.OnPlayerDeath -= HandlePlayerDeath;

        if (playerWin)
        {
            LevelManager.Instance.GameClear();
        }
        else
        {
            playSceneUI.ShowGameOverUI();
        }
        CursorManager.Instance.SetCursor(false);
    }
}
