using System;
using Mono.Cecil;
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
            CalculateTimeBonus();
            OnLevelWin?.Invoke();
        }
    }

    void CalculateTimeBonus()
    {
        float levelClearTime = Time.time - startTime;
        float timeRemaining = clearTime - levelClearTime;

        if (timeRemaining > 0)
        {
            int timeBonus = Mathf.RoundToInt(timeRemaining * scoreTime);
            ScoreManager.Instance.AddScore(timeBonus);
        }
    }
}
