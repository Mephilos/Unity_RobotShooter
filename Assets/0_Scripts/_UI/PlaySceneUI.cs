using UnityEngine;
using TMPro;
using StarterAssets;
using UnityEngine.UI;

public class PlaySceneUI : MonoBehaviour
{
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text accText;
    [SerializeField] GameObject winContainer;
    [SerializeField] GameObject scorePanel;

    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] TMP_Text finalTimeText;
    [SerializeField] TMP_Text finalAccText;

    StarterAssetsInputs starterAssetsInputs;

    void Start()
    {
        starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();

        LevelManager.Instance.OnEnemyCountChanged += UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin += ShowWinUI;
        UpdateEnemyLeft(LevelManager.Instance.GetEnemiesCount());

        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        ScoreManager.Instance.OnAccChanged += UpdateAccUI;
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        UpdateAccUI(ScoreManager.Instance.GetAccuracy());
    }
    void OnDestroy()
    {
        LevelManager.Instance.OnEnemyCountChanged -= UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin -= ShowWinUI;
        ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
        ScoreManager.Instance.OnAccChanged -= UpdateAccUI;
    }
    void ShowWinUI()
    {
        winContainer.SetActive(true);

        int currentScore = ScoreManager.Instance.GetCurrentScore();
        float currentAcc = ScoreManager.Instance.GetAccuracy();
        float currentTime = Time.timeSinceLevelLoad;

        int bestScore = FirebaseManager.Instance.BestScore;
        float bestTime = FirebaseManager.Instance.BestTime;
        float bestAcc = FirebaseManager.Instance.BestAcc;

        DisplayComparisonScore(finalScoreText, "SCORE", currentScore, bestScore, true, "", 0f);
        DisplayComparisonTime(finalTimeText, currentTime, bestTime);
        DisplayComparisonScore(finalAccText, "ACCURACY", currentAcc, bestAcc, true, "%", 0f);

        FirebaseManager.Instance.RenewScore(currentScore, currentTime, currentAcc);

        UnlockCursor();
    }

    void DisplayComparisonScore(TMP_Text textUI, string label, float current, float best, bool isBetter, string suffix = "", float invalidValue = 0f)
    {
        float valueDiff = current - best;
        string comparisonString = "";
        string resultColor = "white";

        if (Mathf.Abs(best - invalidValue) < 0.01f)
        {
            comparisonString = " (New Record)";
            resultColor = "yellow";
        }
        else
        {
            if (Mathf.Abs(valueDiff) < 0.01f)
            {
                comparisonString = " (-)";
                resultColor = "#888888";
            }
            else if ((isBetter && valueDiff > 0) || (!isBetter && valueDiff < 0))
            {
                comparisonString = $" (+{Mathf.Abs(valueDiff):0.##}{suffix})";

                if (!isBetter)
                {
                    comparisonString = $" (-{Mathf.Abs(valueDiff):0.##}{suffix})";
                }
                resultColor = "#00FF00";
            }
            else
            {
                comparisonString = $" ({valueDiff:0.##}{suffix})";
                resultColor = "#FF0000";
            }
        }
        textUI.text = $"{label}: {current:0.##}{suffix} <color={resultColor}>{comparisonString}</color>";
    }

    void DisplayComparisonTime(TMP_Text textUI, float currentTime, float bestTime)
    {
        string timeStr = string.Format("{0:00}:{1:00}", (int)currentTime / 60, (int)currentTime % 60);

        float timeDiff = currentTime - bestTime;
        string comparisonString = "";
        string resultColor = "white";

        if (bestTime > 9000)
        {
            comparisonString = "(New)";
            resultColor = "yellow";
        }
        else if (timeDiff < 0)
        {
            comparisonString = $"(-{Mathf.Abs(timeDiff):0}s)";
            resultColor = "#00FF00";
        }
        else
        {
            comparisonString = $"(+{timeDiff:0}s)";
            resultColor = "#FF0000";
        }
        textUI.text = $"TIME: {timeStr} <color={resultColor}>{comparisonString}</color>";
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

    void UnlockCursor()
    {
        starterAssetsInputs.SetInputBlocked(true);
        starterAssetsInputs.SetCursorState(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
