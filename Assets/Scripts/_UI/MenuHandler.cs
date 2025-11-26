using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;

public class MenuHandler : MonoBehaviour
{
    public enum ButtonType
    {
        Continue,
        Pause,
        NextStage,
        Restart,
        ReturnToMainMenu,
        GameOption,
        Quit,
        PanelClose,
        PanelOpen
    }

    [System.Serializable]
    public struct ButtonMapping
    {
        public string name;
        public Button button;
        public ButtonType type;
        public GameObject panel;
    }

    public List<ButtonMapping> Buttons = new List<ButtonMapping>();

    protected virtual void Start()
    {
        if (GameManager.instance == null)
        {
            Debug.LogWarning($"{gameObject}: GameManager 없음. 메인메뉴부터 실행 ㄱㄱ");
        }

        foreach (var mapping in Buttons)
        {
            mapping.button.onClick.AddListener(() => OnButtonClick(mapping));
        }
    }

    protected virtual void OnButtonClick(ButtonMapping mapping)
    {
        switch (mapping.type)
        {
            case ButtonType.NextStage:
                GameManager.instance.NextScene();
                break;
            case ButtonType.Restart:
                GameManager.instance.RestartButton();
                break;
            case ButtonType.ReturnToMainMenu:
                GameManager.instance.ReturnToMainMenu();
                break;
            case ButtonType.Quit:
                GameManager.instance.QuitGame();
                break;

            case ButtonType.PanelOpen:
                if (mapping.panel != null) mapping.panel.SetActive(true);
                break;
            case ButtonType.PanelClose:
                if (mapping.panel != null) mapping.panel.SetActive(false);
                break;
        }
    }
}
