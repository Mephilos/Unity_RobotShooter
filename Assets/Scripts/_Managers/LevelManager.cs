using System;
using TMPro;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public event Action<int> OnEnemyCountChanged;
    public event Action OnLevelWin;
    int enemiesLeft = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void AdjustEnemiesLeft(int amount)
    {
        enemiesLeft += amount;

        OnEnemyCountChanged?.Invoke(enemiesLeft);
        if (enemiesLeft <= 0)
        {
            OnLevelWin?.Invoke();
        }
    }
}
