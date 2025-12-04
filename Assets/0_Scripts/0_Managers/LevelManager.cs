using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action<int> OnEnemyCountChanged;
    public event Action OnLevelWin;
    int enemiesLeft = 0;
    float startTime;
    float clearTime;
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
        LoadDataFromCSV();
    }

    void LoadDataFromCSV()
    {
        TextAsset csvData = Resources.Load<TextAsset>("TimeData");

        if (csvData == null)
        {
            Debug.LogError("CSV파일 필요");
            return;
        }

        string[] line = csvData.text.Split('\n');
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

        for (int i = 1; i < line.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(line[i])) continue;

            string[] data = line[i].Split(',');

            int leveIndex = int.Parse(data[0]);

            if (leveIndex == currentLevelIndex)
            {
                clearTime = float.Parse(data[1]);
                scoreTime = int.Parse(data[2]);

                return;
            }
        }
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
        CalculateTimeAndAccBonus();

        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        int stageScore = ScoreManager.Instance.GetCurrentScore();
        float stageTime = Time.timeSinceLevelLoad;
        float stageAcc = ScoreManager.Instance.GetAccuracy();

        ScoreManager.Instance.RecordScore(currentLevelIndex, stageScore, stageTime, stageAcc);
        FirebaseManager.Instance.StageRecordSave(currentLevelIndex, stageScore, stageTime, stageAcc);

        OnLevelWin?.Invoke();
    }

    void CalculateTimeAndAccBonus()
    {
        float levelClearTime = Time.time - startTime;
        float timeRemaining = clearTime - levelClearTime;

        if (timeRemaining > 0)
        {
            int timeBonus = Mathf.RoundToInt(timeRemaining * scoreTime);
            ScoreManager.Instance.AddScore(timeBonus);
            Debug.Log($"[TimeBonus] {timeRemaining:F1}<- 남은시간 \n 시간 보너스 점수: {timeBonus}");
        }

        int curTotalScore = ScoreManager.Instance.GetCurrentScore();
        float accuracy = ScoreManager.Instance.GetAccuracy();
        int accBonus = Mathf.RoundToInt(curTotalScore * (accuracy / 100f));

        ScoreManager.Instance.AddScore(accBonus);

        Debug.Log($"정확도 보너스 점수: 정확도{accuracy:F1}% -> 정확도 보너스: {accBonus}");
        Debug.Log($"최종 점수: {ScoreManager.Instance.GetCurrentScore()}");
    }
    public int GetEnemiesCount()
    {
        return enemiesLeft;
    }
}
