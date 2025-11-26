using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.CompilerServices;
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

        OnPauseToggle?.Invoke(IsPause);
    }
    public void RestartButton()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentScene);
    }
    public void NextScene()
    {
        int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextScene);
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(Constants.SCENE_MAIN_MENU);
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("끝 꺼짐");
    }
}
