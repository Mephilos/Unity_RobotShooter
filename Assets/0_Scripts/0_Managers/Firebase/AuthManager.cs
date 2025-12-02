using UnityEngine;
using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    FirebaseAuth firebaseAuth;
    FirebaseUser firebaseUser;

    public FirebaseUser CurrentUser => firebaseUser;
    public string UserId => firebaseUser != null ? firebaseUser.UserId : "";
    public string DisplayName => firebaseUser != null ? firebaseUser.DisplayName : "UnknowPlayer";
    public bool IsFirebaseReady { get; private set; } = false;
    public event Action<FirebaseUser> OnLoginSuccess;
    public event Action<string> OnLoginFailed;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeAuth()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                firebaseAuth = FirebaseAuth.DefaultInstance;
                firebaseAuth.StateChanged += AuthStateChanged;

                IsFirebaseReady = true;

                AuthStateChanged(this, null);

                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"파이어베이스 연결 실패: {dependencyStatus}");
            }
        });
    }

    void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (firebaseAuth.CurrentUser != firebaseUser)
        {
            bool signedIn = firebaseUser != firebaseAuth.CurrentUser && firebaseAuth.CurrentUser != null;

            if (!signedIn && firebaseUser != null)
            {
                Debug.Log("로그아웃: " + firebaseUser.UserId);
            }

            firebaseUser = firebaseAuth.CurrentUser;

            if (signedIn)
            {
                Debug.Log($"로그인 감지: {firebaseUser.UserId}");

                OnLoginSuccess?.Invoke(firebaseUser);
            }
        }
    }

    public void Register(string email, string password, string nickName)
    {
        Debug.Log("회원가입 시도");
        firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("회원가입 실패: " + task.Exception);
                OnLoginFailed?.Invoke("회원가입 실패: " + task.Exception?.Message);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log("회원가입 성공");

            UpdateProfile(nickName);
        });
    }

    public void Login(string email, string password)
    {
        Debug.Log("로그인 시도");
        firebaseAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("로그인 실패: " + task.Exception);
                OnLoginFailed?.Invoke("로그인 실패");
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log($"로그인 성공 {user.DisplayName}");
        });
    }

    public void SignInAnonymously()
    {
        Debug.Log("익명으로 로그인 합니다");

        firebaseAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("익명 로그인 에러: " + task.Exception);
                OnLoginFailed?.Invoke(task.Exception?.Message);
                return;
            }

            FirebaseUser newUser = task.Result.User;

            if (string.IsNullOrEmpty(newUser.DisplayName))
            {
                SetGuestName();
            }
        });
    }

    void SetGuestName()
    {
        string randomSuffix = UnityEngine.Random.Range(10000, 99999).ToString();
        string newName = $"Guest_{randomSuffix}";
        UpdateProfile(newName);
    }

    public void UpdateProfile(string newNickname)
    {
        UserProfile profile = new UserProfile { DisplayName = newNickname };

        firebaseUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("닉네임 설정 실패");
                return;
            }
            Debug.Log($"닉네임 업데이트 완료: {newNickname}");

            OnLoginSuccess?.Invoke(firebaseUser);
        });
    }

    public void SignOut()
    {
        firebaseAuth.SignOut();
    }

    void OnDestroy()
    {
        firebaseAuth.StateChanged -= AuthStateChanged;
    }
}
