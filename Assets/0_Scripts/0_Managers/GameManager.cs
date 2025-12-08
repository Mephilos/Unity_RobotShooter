using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public bool IsPause { get; private set; } = false;

    public event Action<bool> OnPauseToggle;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
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
        InitPause();
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }
    public void NextScene()
    {
        InitPause();

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
        InitPause();
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
}
