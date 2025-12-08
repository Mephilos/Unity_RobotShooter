using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action<int> OnEnemyCountChanged;
    public event Action<int, float, float, int, bool> OnStageClearData;
    int enemiesLeft = 0;
    float startTime;

    float limitTime;
    int scoreTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        startTime = Time.time;
        InitStageClearData();
    }

    void InitStageClearData()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        StageClearTimeData data = CSVManager.Instance.GetStageClearTimeData(currentLevelIndex);

        limitTime = data.limitTime;
        scoreTime = data.scoreTime;
    }

    public void AdjustEnemiesLeft(int amount)
    {
        enemiesLeft += amount;

        OnEnemyCountChanged?.Invoke(enemiesLeft);
        if (enemiesLeft <= 0)
        {
            Debug.Log("승리 호출");
            ProcessStageClearScore();
        }
    }

    void ProcessStageClearScore()
    {
        float levelClearTime = Time.time - startTime;
        ScoreManager.Instance.CalculateTimeAndAccBonus(levelClearTime, limitTime, scoreTime);

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int stageScore = ScoreManager.Instance.GetCurrentScore();
        float stageAcc = ScoreManager.Instance.GetAccuracy();

        ScoreManager.Instance.RecordScore(currentLevelIndex, stageScore, levelClearTime, stageAcc);
        FirebaseManager.Instance.StageScoreSave(currentLevelIndex, stageScore, levelClearTime, stageAcc, (isNewScore, dbBestScore) =>
        {
            int bestScoreToDisplay = isNewScore ? stageScore : dbBestScore;
            OnStageClearData?.Invoke(stageScore, levelClearTime, stageAcc, bestScoreToDisplay, isNewScore);
        });
    }

    public int GetEnemiesCount()
    {
        return enemiesLeft;
    }
}
