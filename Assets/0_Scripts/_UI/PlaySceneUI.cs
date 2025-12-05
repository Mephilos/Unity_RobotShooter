using UnityEngine;
using TMPro;
using StarterAssets;
using UnityEngine.SceneManagement;

public class PlaySceneUI : MonoBehaviour
{
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text accText;
    [SerializeField] GameObject winContainer;

    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] TMP_Text finalTimeText;
    [SerializeField] TMP_Text finalAccText;
    [SerializeField] LeaderboardHandler leaderboardHandler;
    StarterAssetsInputs starterAssetsInputs;

    void Start()
    {
        starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();

        LevelManager.Instance.OnEnemyCountChanged += UpdateEnemyLeft;
        LevelManager.Instance.OnStageClearData += ShowWinUI;
        UpdateEnemyLeft(LevelManager.Instance.GetEnemiesCount());

        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        ScoreManager.Instance.OnAccChanged += UpdateAccUI;
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        UpdateAccUI(ScoreManager.Instance.GetAccuracy());
    }
    void OnDestroy()
    {
        LevelManager.Instance.OnEnemyCountChanged -= UpdateEnemyLeft;
        LevelManager.Instance.OnStageClearData -= ShowWinUI;
        ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
        ScoreManager.Instance.OnAccChanged -= UpdateAccUI;
    }
    void ShowWinUI(int stageScore, float levelClearTime, float stageAcc, int bestScore, bool isNewScore)
    {
        winContainer.SetActive(true);

        DisplayComparisonScore(finalScoreText, "SCORE", stageScore, bestScore, isNewScore, "", 0f);

        string timeStr = string.Format("{0:00}:{1:00}", (int)levelClearTime / 60, (int)levelClearTime % 60);
        finalTimeText.text = $"TIME: {timeStr}";
        finalAccText.text = $"ACCURACY: {stageAcc:F1}%";

        int currentStageIndex = SceneManager.GetActiveScene().buildIndex;

        if (leaderboardHandler != null)
        {
            leaderboardHandler.LoadStageLeaderboard(currentStageIndex);
        }
        UnlockCursor();
    }

    void DisplayComparisonScore(TMP_Text textUI, string label, float current, float best, bool isNewScore, string suffix = "", float invalidValue = 0f)
    {
        float valueDiff = current - best;
        string comparisonString = "";
        string resultColor = "white";

        if (isNewScore)
        {
            comparisonString = "(New)";
            resultColor = "yellow";
        }

        else if (Mathf.Abs(valueDiff) < 0.01f)
        {
            comparisonString = " (-)";
            resultColor = "#888888";
        }
        else if (valueDiff > 0)
        {
            comparisonString = $" (-{Mathf.Abs(valueDiff):0.##}{suffix})";
            resultColor = "#00FF00";
        }
        else
        {
            comparisonString = $" ({valueDiff:0.##}{suffix})";
            resultColor = "#FF0000";
        }

        textUI.text = $"{label}: {current:0.##}{suffix} <color={resultColor}>{comparisonString}</color>";
    }

    // void DisplayComparisonTime(TMP_Text textUI, float currentTime, float bestTime)
    // {
    //     string timeStr = string.Format("{0:00}:{1:00}", (int)currentTime / 60, (int)currentTime % 60);

    //     float timeDiff = currentTime - bestTime;
    //     string comparisonString = "";
    //     string resultColor = "white";

    //     if (bestTime > 9000)
    //     {
    //         comparisonString = "(New)";
    //         resultColor = "yellow";
    //     }
    //     else if (timeDiff < 0)
    //     {
    //         comparisonString = $"(-{Mathf.Abs(timeDiff):0}s)";
    //         resultColor = "#00FF00";
    //     }
    //     else
    //     {
    //         comparisonString = $"(+{timeDiff:0}s)";
    //         resultColor = "#FF0000";
    //     }
    //     textUI.text = $"TIME: {timeStr} <color=\"{resultColor}\">{comparisonString}</color>";
    // }

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
