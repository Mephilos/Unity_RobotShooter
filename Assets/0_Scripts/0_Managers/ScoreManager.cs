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
        OnScoreChanged?.Invoke(currentScore);
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

    public (int totalScore, float totalTime, float avgAcc) GetTotalStats()
    {
        int totalScore = stageScores.Sum(r => r.score);
        float totalTime = stageScores.Sum(r => r.time);
        float avgAcc = stageScores.Average(r => r.acc);

        return (totalScore, totalTime, avgAcc);
    }
}
