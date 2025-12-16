using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    public bool IsPause { get; private set; } = false;
    public PlayerHealth Player { get; private set; }
    public event Action<bool> OnPauseToggle;
    public event Action<PlayerHealth> OnPlayerRegistered;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Player = null;
        InitPause();
    }

    public void PauseToggle()
    {
        IsPause = !IsPause;
        Time.timeScale = IsPause ? 0f : 1f;
        Cursor.lockState = IsPause ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsPause;
        CursorManager.Instance.SetCursor(!IsPause);
        OnPauseToggle?.Invoke(IsPause);
    }

    public void RestartButton()
    {
        LevelManager.Instance.IsStageActive = false;
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }

    public void NextScene()
    {
        LevelManager.Instance.IsStageActive = false;
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("다음 씬 없음. 메인메뉴로 돌아감");
            ReturnToMainMenu();
        }
    }

    public void ReturnToMainMenu()
    {
        LevelManager.Instance.IsStageActive = false;
        SceneManager.LoadScene(Constants.SCENE_MAIN_MENU);

        CursorManager.Instance.SetCursor(false);
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("끝 꺼짐");
    }
    void InitPause()
    {
        IsPause = false;
        Time.timeScale = 1f;
    }
    public void FindPlayer(PlayerHealth playerHealth)
    {
        Player = playerHealth;
        OnPlayerRegistered?.Invoke(Player);
    }
}
