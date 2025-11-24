using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayMenuHandler : MonoBehaviour
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] Button restartButton;
    [SerializeField] Button returnMainMenuButton;
    [SerializeField] Button quitGameButton;

    void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(() => GameManager.instance.RestartButton());
        if (returnMainMenuButton != null) returnMainMenuButton.onClick.AddListener(() => GameManager.instance.ReturnToMainMenu());
        if (quitGameButton != null) quitGameButton.onClick.AddListener(() => GameManager.instance.QuitGame());
    }
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
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
    }
}
