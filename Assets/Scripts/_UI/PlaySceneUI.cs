using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaySceneUI : MonoBehaviour
{
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] GameObject winText;

    void Start()
    {
        UpdateEnemyLeft(0);
        LevelManager.Instance.OnEnemyCountChanged += UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin += ShowWinUI;
    }
    void OnDestroy()
    {
        LevelManager.Instance.OnEnemyCountChanged -= UpdateEnemyLeft;
        LevelManager.Instance.OnLevelWin -= ShowWinUI;
    }
    void UpdateEnemyLeft(int count)
    {
        enemiesLeftText.text = Constants.ENEMIES_LEFT_STRING + count.ToString("D2");
    }
    void ShowWinUI()
    {
        winText.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
