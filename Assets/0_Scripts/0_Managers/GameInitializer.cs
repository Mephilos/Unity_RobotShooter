using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{

    [SerializeField] GameObject soundManagerPrefab;
    [SerializeField] GameObject poolManagerPrefab;
    [SerializeField] GameObject csvManagerPrefab;
    [SerializeField] GameObject firebaseManagerPrefab;
    [SerializeField] GameObject gameManagerPrefab;
    [SerializeField] GameObject cursorManagerPrefab;

    void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep; // 화면 꺼짐 방지
        StartCoroutine(InitializeGame());
    }

    IEnumerator InitializeGame()
    {
        Debug.Log("매니져 초기화");

        yield return StartCoroutine(InitManager(csvManagerPrefab, "CSV Manager"));
        yield return StartCoroutine(InitManager(soundManagerPrefab, "Sound Manager"));
        yield return StartCoroutine(InitManager(poolManagerPrefab, "Pool Manager"));
        yield return StartCoroutine(InitManager(cursorManagerPrefab, "Cursor Manager"));
        yield return StartCoroutine(InitManager(firebaseManagerPrefab, "Firebase/Auth Manager"));

        while (!AuthManager.Instance.IsFirebaseReady)
        {
            Debug.Log("파이어베이스 연결 중");
            yield return null;
        }
        yield return StartCoroutine(InitManager(gameManagerPrefab, "Game Manager"));

        Debug.Log("초기화 끝");

        SceneManager.LoadScene(Constants.SCENE_MAIN_MENU);
    }

    IEnumerator InitManager(GameObject prefab, string name)
    {
        Instantiate(prefab);
        yield return null;

        Debug.Log($"{name} 초기화 완료.");
    }
}
