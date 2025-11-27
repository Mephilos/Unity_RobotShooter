using UnityEngine;
using TMPro;
using StarterAssets;

public class PlaySceneUI : MonoBehaviour
{
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text accText;
    [SerializeField] GameObject winText;
    StarterAssetsInputs starterAssetsInputs;

    void Start()
    {
        starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();
        LevelManager.Instance.OnEnemyCountChanged += UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin += ShowWinUI;
        UpdateEnemyLeft(0);

        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        ScoreManager.Instance.OnAccChanged += UpdateAccUI;
        UpdateAccUI(ScoreManager.Instance.GetAccuracy());
    }
    void OnDestroy()
    {
        LevelManager.Instance.OnEnemyCountChanged -= UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin -= ShowWinUI;
        ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
        ScoreManager.Instance.OnAccChanged -= UpdateAccUI;
    }
    void UpdateEnemyLeft(int count)
    {
        enemiesLeftText.text = Constants.ENEMIES_LEFT_STRING + count.ToString("D2");
    }
    void UpdateScoreUI(int score)
    {
        scoreText.text = Constants.SCORE_STRING + $"{score:N0}";
    }
    void UpdateAccUI(float acc)
    {
        accText.text = Constants.ACC_STRING + $"{acc:F1}%";
    }
    void ShowWinUI()
    {
        winText.SetActive(true);
        Time.timeScale = 0f;

        starterAssetsInputs.SetInputBlocked(true);
        starterAssetsInputs.SetCursorState(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
