using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayMenuHandler : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] Button restartButton;
    [SerializeField] Button returnMainMenuButton;
    [SerializeField] Button quitGameButton;

    [SerializeField] Button gameOverQuitButton;
    [SerializeField] Button gameOverRestartButton;

    StarterAssetsInputs playerInputs;

    void Awake()
    {
        Time.timeScale = 1.0f;
    }
    void Start()
    {
        playerInputs = FindFirstObjectByType<StarterAssetsInputs>();

        if (restartButton != null) restartButton.onClick.AddListener(() => GameManager.instance.RestartButton());
        if (returnMainMenuButton != null) returnMainMenuButton.onClick.AddListener(() => GameManager.instance.ReturnToMainMenu());
        if (quitGameButton != null) quitGameButton.onClick.AddListener(() => GameManager.instance.QuitGame());

    }
    void Update()
    {
        if (playerInputs == null) return;

        if (playerInputs.pause)
        {
            playerInputs.pause = false;
            TogglePause();
        }
    }

    public void TogglePause()
    {
        bool isActive = !pausePanel.activeSelf;
        pausePanel.SetActive(isActive);

        Time.timeScale = isActive ? 0 : 1;

        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isActive;

        if (playerInputs != null)
        {
            playerInputs.SetInputBlocked(isActive);
        }
    }
}
