using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SocialPlatforms.Impl;

public class PlaySceneUI : MonoBehaviour
{
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] GameObject winText;

    void Start()
    {
        LevelManager.Instance.OnEnemyCountChanged += UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin += ShowWinUI;
        UpdateEnemyLeft(0);

        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());

    }
    void OnDestroy()
    {
        LevelManager.Instance.OnEnemyCountChanged -= UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin -= ShowWinUI;
        ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
    }
    void UpdateEnemyLeft(int count)
    {
        enemiesLeftText.text = Constants.ENEMIES_LEFT_STRING + count.ToString("D2");
    }
    void UpdateScoreUI(int score)
    {
        scoreText.text = Constants.SCORE_STRING + $"{score:N0}";
    }
    void ShowWinUI()
    {
        winText.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
