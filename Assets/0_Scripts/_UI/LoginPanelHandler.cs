using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class LoginPanelHandler : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TMP_InputField nickNameInputField;
    [SerializeField] Button loginButton;
    [SerializeField] Button registerButton;
    [SerializeField] GameObject statusPanel;
    [SerializeField] TMP_Text statusText;
    float statusPanelDelay = 1.5f;
    WaitForSeconds wait;

    Coroutine currentRoutine;

    void Awake()
    {
        wait = new WaitForSeconds(statusPanelDelay);
    }
    void Start()
    {
        loginButton.onClick.AddListener(OnClickLogin);
        registerButton.onClick.AddListener(OnClickRegister);
        statusText.text = "";
    }

    public void OnClickLogin()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateStatus("아이디, 비밀번호 입력 필요");
            return;
        }
        UpdateStatus("로그인 시도 중");
        AuthManager.Instance.Login(email, password);
    }

    public void OnClickRegister()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;
        string nickName = nickNameInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(nickName))
        {
            UpdateStatus("가입 정보를 입력해주세요");
            return;
        }
        UpdateStatus("회원가입 요청 중");
        AuthManager.Instance.Register(email, password, nickName);
    }

    void UpdateStatus(string msg)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        statusPanel.SetActive(true);
        statusText.text = msg;
    }

    void OffStatusPanel()
    {
        statusPanel.SetActive(false);
    }

    void HandlerSuccess(Firebase.Auth.FirebaseUser user)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        currentRoutine = StartCoroutine(SuccessRoutine());
    }

    void HandlerFail(string err)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
        UpdateStatus($"오류 발생: {err}");
    }
    IEnumerator SuccessRoutine()
    {
        statusText.text = "로그인 성공";

        yield return wait;

        OffStatusPanel();
    }
    void OnEnable()
    {
        AuthManager.Instance.OnLoginSuccess += HandlerSuccess;
        AuthManager.Instance.OnLoginFailed += HandlerFail;
    }

    void OnDisable()
    {
        AuthManager.Instance.OnLoginSuccess -= HandlerSuccess;
        AuthManager.Instance.OnLoginFailed -= HandlerFail;
    }
}
