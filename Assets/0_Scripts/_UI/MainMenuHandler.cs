using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuHandler : MenuHandler
{
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject loginPanel;
    [SerializeField] GameObject leaderBoardPanel;

    [SerializeField] GameObject loginButton;
    [SerializeField] GameObject logoutButton;

    [SerializeField] InputActionReference cancelDetec;

    void OnEnable()
    {
        cancelDetec.action.Enable();
        cancelDetec.action.performed += OnCancelInput;

        AuthManager.Instance.OnLoginSuccess += RefreshAuthUI;
    }
    void OnDisable()
    {
        cancelDetec.action.performed -= OnCancelInput;
        cancelDetec.action.Disable();
    }

    void RefreshAuthUI(Firebase.Auth.FirebaseUser firebaseUser)
    {
        bool isLogin = false;
        if (firebaseUser != null)
        {
            isLogin = true;
        }
        logoutButton.SetActive(isLogin);
        loginButton.SetActive(!isLogin);
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
