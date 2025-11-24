using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.UI;
using Unity.VisualScripting;
public class MainMenuHandler : MonoBehaviour
{
    [SerializeField] GameObject optionPanel;
    [SerializeField] Button gameStartButton;
    [SerializeField] Button gameOptionButton;
    [SerializeField] Button quitButton;

    void Start()
    {
        if (gameStartButton != null)
        {
            gameStartButton.onClick.AddListener(() => GameManager.instance.NextScene());
        }
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(() => GameManager.instance.QuitGame());
        }
    }

}
