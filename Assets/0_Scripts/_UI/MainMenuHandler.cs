using System.Collections;
using Firebase.Auth;
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
    Coroutine authCheckRoutine;


    void OnEnable()
    {
        cancelDetec.action.Enable();
        cancelDetec.action.performed += OnCancelInput;

        authCheckRoutine = StartCoroutine(AuthInitWaitRoutine());
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

        Debug.Log($"로그인 여부: {isLogin} (유저: {firebaseUser?.UserId}, 익명: {firebaseUser?.IsAnonymous})");
        logoutButton.SetActive(isLogin);
        loginButton.SetActive(!isLogin);
    }

    IEnumerator AuthInitWaitRoutine()
    {
        loginButton.SetActive(false);
        logoutButton.SetActive(false);

        while (AuthManager.Instance == null)
        {
            yield return null;
        }
        AuthManager.Instance.OnLoginSuccess += RefreshAuthUI;
        AuthManager.Instance.OnLogout += OnLogout;
        while (!AuthManager.Instance.IsFirebaseReady)
        {
            yield return null;
        }
        RefreshAuthUI(AuthManager.Instance.CurrentUser);
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
