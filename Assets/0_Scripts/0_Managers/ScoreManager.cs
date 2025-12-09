using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StagesScore
{
    public int stageIndex;
    public int score;
    public float time;
    public float acc;
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public List<StagesScore> stageScores = new List<StagesScore>();
    public event Action<int> OnScoreChanged;
    public event Action<float> OnAccChanged;

    int currentScore = 0;
    int totalShots = 0;
    int totalHits = 0;

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

    public void AddScore(int amount)
    {
        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public void RestoreScore()
    {
        currentScore = 0;
        totalShots = 0;
        totalHits = 0;
        OnScoreChanged?.Invoke(currentScore);
        OnAccChanged?.Invoke(0f);
    }

    public void ReportShot()
    {
        totalShots++;
        OnAccChanged?.Invoke(GetAccuracy());
    }

    public void ReportHit()
    {
        totalHits++;
        OnAccChanged?.Invoke(GetAccuracy());
    }

    public float GetAccuracy()
    {
        if (totalShots == 0) return 0f;

        return (float)totalHits / totalShots * 100f;
    }

    public void RecordScore(int stageIndex, int score, float time, float acc)
    {
        StagesScore newScore = new StagesScore
        {
            stageIndex = stageIndex,
            score = score,
            time = time,
            acc = acc
        };

        stageScores.Add(newScore);
    }

    public void CalculateTimeAndAccBonus(float clearTime, float timeLimit, int scorePerSec)
    {
        float timeRemaining = timeLimit - clearTime;

        if (timeRemaining > 0)
        {
            int timeBonus = Mathf.RoundToInt(timeRemaining * scorePerSec);
            AddScore(timeBonus);
            Debug.Log($"[TimeBonus] {timeRemaining:F1}<- 남은시간 \n 시간 보너스 점수: {timeBonus}");
        }

        float accuracy = GetAccuracy();
        int accBonus = Mathf.RoundToInt(currentScore * (accuracy / 100f));

        AddScore(accBonus);

        Debug.Log($"정확도 보너스 점수: 정확도{accuracy:F1}% -> 정확도 보너스: {accBonus}");
        Debug.Log($"최종 점수: {GetCurrentScore()}");
    }

    public (int totalScore, float totalTime, float avgAcc) GetTotalStats()
    {
        return CalculateStats(this.stageScores);
    }
    public static (int totalScore, float totalTime, float avgAcc) CalculateStats(List<StagesScore> scoreList)
    {
        int totalScore = scoreList.Sum(r => r.score);
        float totalTime = scoreList.Sum(r => r.time);
        float avgAcc = scoreList.Average(r => r.acc);

        return (totalScore, totalTime, avgAcc);
    }
}
