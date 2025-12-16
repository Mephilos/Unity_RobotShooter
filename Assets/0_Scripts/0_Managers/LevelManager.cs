using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public bool IsStageActive { get; set; } = false;
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == Constants.BOOT_SCENE || scene.buildIndex == Constants.SCENE_MAIN_MENU)
            return;

        InitializeStage();
    }

    void InitializeStage()
    {
        startTime = Time.time;
        ScoreManager.Instance.RestoreScore();
        InitStageClearData();

        OnEnemyCountChanged?.Invoke(enemiesLeft);
        IsStageActive = true;
    }

    void InitStageClearData()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex - Constants.SCENE_MAIN_MENU;

        StageClearTimeData data = CSVManager.Instance.GetStageClearTimeData(currentLevelIndex);

        limitTime = data.limitTime;
        scoreTime = data.scoreTime;
    }

    public void AdjustEnemiesLeft(int amount)
    {
        enemiesLeft += amount;

        OnEnemyCountChanged?.Invoke(enemiesLeft);
        if (enemiesLeft <= 0 && IsStageActive)
        {
            Debug.Log("승리 호출");
            GameClear();
        }
    }

    public void GameClear()
    {
        IsStageActive = false;
        ProcessStageClearScore();
    }
    void ProcessStageClearScore()
    {
        float levelClearTime = Time.time - startTime;
        ScoreManager.Instance.CalculateTotalScoreAndAdd(levelClearTime, limitTime, scoreTime);

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex - Constants.SCENE_MAIN_MENU;
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
