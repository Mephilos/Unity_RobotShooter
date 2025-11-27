using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public event Action<int> OnScoreChanged;
    public event Action<float> OnAccChanged;

    int currentScore = 0;
    int totalShots = 0;
    int totalHits = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
}
