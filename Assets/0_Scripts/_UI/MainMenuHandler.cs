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
        AuthManager.Instance.OnLoginSuccess += RefreshAuthUI;
        AuthManager.Instance.OnLogout += OnLogout;

        cancelDetec.action.Enable();
        cancelDetec.action.performed += OnCancelInput;
    }

    void OnDisable()
    {
        cancelDetec.action.performed -= OnCancelInput;
        cancelDetec.action.Disable();

        AuthManager.Instance.OnLoginSuccess -= RefreshAuthUI;
        AuthManager.Instance.OnLogout -= OnLogout;
    }

    void RefreshAuthUI(Firebase.Auth.FirebaseUser firebaseUser)
    {
        bool isLogin = false;
        if (firebaseUser != null && !firebaseUser.IsAnonymous)
        {
            isLogin = true;
        }

        Debug.Log($"[UI 갱신] 찐로그인 여부: {isLogin} (유저: {firebaseUser?.UserId}, 익명: {firebaseUser?.IsAnonymous})");
        logoutButton.SetActive(isLogin);
        loginButton.SetActive(!isLogin);
    }
    void OnLogout()
    {
        RefreshAuthUI(null);
        loginPanel.SetActive(false);
        leaderBoardPanel.SetActive(false);
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
