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
    float statusPanelDelayErr = 3f;
    WaitForSeconds waitSuccess;
    WaitForSeconds waitErr;

    Coroutine currentRoutine;

    void Awake()
    {
        waitSuccess = new WaitForSeconds(statusPanelDelay);
        waitErr = new WaitForSeconds(statusPanelDelayErr);
    }

    void OnEnable()
    {
        AuthManager.Instance.OnLoginSuccess += HandlerSuccess;
        AuthManager.Instance.OnLoginFailed += HandlerFail;

        statusPanel.SetActive(false);
        statusText.text = "";
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
        CheckCurRoutineAndStop();
        statusPanel.SetActive(true);
        statusText.text = msg;
    }

    void OffStatusPanel()
    {
        CheckCurRoutineAndStop();
        statusPanel.SetActive(false);
    }

    void HandlerSuccess(Firebase.Auth.FirebaseUser user)
    {
        if (user != null && user.IsAnonymous) return;
        CheckCurRoutineAndStop();
        currentRoutine = StartCoroutine(SuccessRoutine());
    }

    void HandlerFail(string err)
    {
        CheckCurRoutineAndStop();
        currentRoutine = StartCoroutine(FailRoutine(err));
    }

    IEnumerator SuccessRoutine()
    {
        statusText.text = "로그인 성공";

        yield return waitSuccess;

        OffStatusPanel();
        gameObject.SetActive(false);
    }

    IEnumerator FailRoutine(string msg)
    {
        statusText.text = msg;

        yield return waitErr;

        OffStatusPanel();
    }

    void OnDisable()
    {
        AuthManager.Instance.OnLoginSuccess -= HandlerSuccess;
        AuthManager.Instance.OnLoginFailed -= HandlerFail;

        CheckCurRoutineAndStop();
        statusPanel.SetActive(false);
    }

    void CheckCurRoutineAndStop()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }
    }
}
