using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuHandler : MenuHandler
{
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject leaderBoardPanel;
    [SerializeField] InputActionReference cancelDetec;

    void OnEnable()
    {
        cancelDetec.action.Enable();
        cancelDetec.action.performed += OnCancelInput;
    }
    void OnDisable()
    {
        cancelDetec.action.performed -= OnCancelInput;
        cancelDetec.action.Disable();
    }
    void OnCancelInput(InputAction.CallbackContext callback)
    {
        if (optionPanel != null && optionPanel.activeSelf)
        {
            optionPanel.SetActive(false);
        }
        if (loginPanel != null && loginPanel.activeSelf)
        {
            loginPanel.SetActive(false);
        }
        if (leaderBoardPanel != null && leaderBoardPanel.activeSelf)
        {
            leaderBoardPanel.SetActive(false);
        }
    }
}
