using UnityEngine;
using TMPro;
using StarterAssets;
using UnityEngine.SceneManagement;

public class PlaySceneUI : MonoBehaviour
{
    [SerializeField] TMP_Text ammoText;
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text accText;
    [SerializeField] TMP_Text hpText;
    [SerializeField] TMP_Text finalScoreText;
    [SerializeField] TMP_Text finalTimeText;
    [SerializeField] TMP_Text finalAccText;
    [SerializeField] TMP_Text enemyScoreText;
    [SerializeField] PlayerSpawner playerSpawner;
    [SerializeField] LeaderboardHandler leaderboardHandler;
    [SerializeField] GameObject winContainer;
    [SerializeField] GameObject gameOverContainer;
    ActiveWeapon activeWeapon;
    PlayerHealth playerHealth;

    void Start()
    {
        LevelManager.Instance.OnEnemyCountChanged += UpdateEnemyLeft;
        LevelManager.Instance.OnStageClearData += ShowWinUI;
        UpdateEnemyLeft(LevelManager.Instance.GetEnemiesCount());

        ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
        ScoreManager.Instance.OnAccChanged += UpdateAccUI;
        UpdateScoreUI(ScoreManager.Instance.GetCurrentScore());
        UpdateAccUI(ScoreManager.Instance.GetAccuracy());

        GameManager.Instance.OnPlayerRegistered += HandleRegister;
    }

    void HandleRegister(PlayerHealth playerHealth, Transform transform)
    {
        PlayerOverlayHandler playerOverlay = playerHealth.GetComponent<PlayerOverlayHandler>();
        BindPlayerHealth(playerHealth);
        BindWeapon(playerOverlay.ActiveWeapon);
    }

    void BindPlayerHealth(PlayerHealth playerHealth)
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdatePlayerHP;
            playerHealth.OnPlayerDeath -= ShowGameOverUI;
        }
        this.playerHealth = playerHealth;
        this.playerHealth.OnHealthChanged += UpdatePlayerHP;
        bool isDeathMatchMode = playerSpawner.IsDeathMatchMode;

        if (!isDeathMatchMode)
        {
            this.playerHealth.OnPlayerDeath += ShowGameOverUI;
        }

        UpdatePlayerHP(playerHealth.CurrentHP, playerHealth.MaxHP);
    }

    public void BindWeapon(ActiveWeapon newWeapon)
    {
        if (activeWeapon != null)
        {
            activeWeapon.OnAmmoChange -= UpdateAmmoUI;
        }

        activeWeapon = newWeapon;

        if (activeWeapon != null)
        {
            activeWeapon.OnAmmoChange += UpdateAmmoUI;
            var (curr, max) = activeWeapon.GetAmmo();
            UpdateAmmoUI(curr, max);
        }
    }



    void UpdatePlayerHP(int currentHp, int maxHp)
    {
        hpText.text = $"{currentHp}";
    }

    void OnDestroy()
    {
        activeWeapon.OnAmmoChange -= UpdateAmmoUI;

        LevelManager.Instance.OnEnemyCountChanged -= UpdateEnemyLeft;
        LevelManager.Instance.OnStageClearData -= ShowWinUI;

        ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
        ScoreManager.Instance.OnAccChanged -= UpdateAccUI;
    }

    void UpdateAmmoUI(int currentAmmo, int maxAmmo)
    {
        ammoText.text = currentAmmo.ToString("D2");
    }

    void ShowWinUI(int stageScore, float levelClearTime, float stageAcc, int bestScore, bool isNewScore)
    {
        winContainer.SetActive(true);

        DisplayComparisonScore(finalScoreText, "SCORE", stageScore, bestScore, isNewScore, "", 0f);

        string timeStr = string.Format("{0:00}:{1:00}", (int)levelClearTime / 60, (int)levelClearTime % 60);
        finalTimeText.text = $"TIME: {timeStr}";
        finalAccText.text = $"ACCURACY: {stageAcc:F1}%";

        int currentStageIndex = SceneManager.GetActiveScene().buildIndex - Constants.SCENE_MAIN_MENU;

        if (leaderboardHandler != null)
        {
            leaderboardHandler.LoadStageLeaderboard(currentStageIndex);
        }
        CursorManager.Instance.SetCursor(false);
    }

    public void ShowGameOverUI()
    {
        gameOverContainer.SetActive(true);
        CursorManager.Instance.SetCursor(false);
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

    public void UpdateDeathMatchScore(int playerScore, int enemyScore, int targetScore)
    {
        if (scoreText != null)
            scoreText.text = $"PLAYER: {playerScore} / {targetScore}";

        if (enemyScoreText != null)
            enemyScoreText.text = $"ENEMY: {enemyScore} / {targetScore}";
    }
}
