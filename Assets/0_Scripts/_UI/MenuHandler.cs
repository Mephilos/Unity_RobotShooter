using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class MenuHandler : MonoBehaviour
{
    public enum ButtonType
    {
        None1,
        None2,
        NextStage,
        Restart,
        ReturnToMainMenu,
        GameOption,
        Quit,
        PanelClose,
        PanelOpen,
        Login,
        LogOut,
    }

    [Serializable]
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
        if (GameManager.Instance == null)
        {
            Debug.LogWarning($"{gameObject}: GameManager 없음");
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
                GameManager.Instance.NextScene();
                break;
            case ButtonType.Restart:
                GameManager.Instance.RestartButton();
                break;
            case ButtonType.ReturnToMainMenu:
                GameManager.Instance.ReturnToMainMenu();
                break;
            case ButtonType.Quit:
                GameManager.Instance.QuitGame();
                break;

            case ButtonType.PanelOpen:
            case ButtonType.Login:
            case ButtonType.GameOption:
                if (mapping.panel != null) mapping.panel.SetActive(true);
                break;
            case ButtonType.PanelClose:
                if (mapping.panel != null) mapping.panel.SetActive(false);
                break;

            case ButtonType.LogOut:
                AuthManager.Instance.SignOut();
                break;
        }
    }
}
