using UnityEngine;
using System;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }
    public FirebaseUser CurrentUser => firebaseUser;
    public string UserId => firebaseUser != null ? firebaseUser.UserId : "";
    public string DisplayName
    {
        get
        {
            if (!string.IsNullOrEmpty(localDisplayName)) return localDisplayName;
            if (firebaseUser != null && !string.IsNullOrEmpty(firebaseUser.DisplayName))
            {
                return firebaseUser.DisplayName;
            }
            else
            {
                return "UnknownPlayer";
            }
        }
    }
    public bool IsFirebaseReady { get; private set; } = false;

    public event Action<FirebaseUser> OnLoginSuccess;
    public event Action<string> OnLoginFailed;
    public event Action OnLogout;

    FirebaseAuth firebaseAuth;
    FirebaseUser firebaseUser;
    string localDisplayName;
    string currentUserId;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAuth();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void InitializeAuth()
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

                if (firebaseAuth.CurrentUser == null)
                {
                    SignInAnonymously();
                }
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
            firebaseUser = firebaseAuth.CurrentUser;

            if (firebaseUser != null)
            {
                string newUserId = firebaseUser.UserId;
                string newDisplayName = firebaseUser.DisplayName;

                if (currentUserId != newUserId)
                {
                    currentUserId = newUserId;
                    localDisplayName = newDisplayName;
                }
                else
                {
                    if (!string.IsNullOrEmpty(newDisplayName))
                    {
                        localDisplayName = newDisplayName;
                    }
                }

                if (firebaseUser.IsAnonymous && string.IsNullOrEmpty(localDisplayName))
                {
                    Debug.Log("이름부여 시도");
                    SetGuestName();
                }
                Debug.Log($"로그인 감지 (StateChanged): {firebaseUser.UserId}");
                OnLoginSuccess?.Invoke(firebaseUser);
            }
            else
            {
                currentUserId = "";
                localDisplayName = null;
                Debug.Log("로그아웃 감지");
                OnLogout?.Invoke();
            }
        }
    }

    public void Register(string email, string password, string nickName)
    {
        if (!IsFirebaseReady) return;

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
            Debug.Log($"회원가입 성공{newUser.DisplayName}");
            OnLoginSuccess?.Invoke(newUser);
            UpdateProfile(nickName);
        });
    }

    public void Login(string email, string password)
    {
        if (!IsFirebaseReady) return;

        Debug.Log("로그인 시도");
        firebaseAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                OnLoginFailed?.Invoke("로그인 실패");
                return;
            }

            FirebaseUser loginUser = task.Result.User;
            localDisplayName = loginUser.DisplayName;
            Debug.Log($"로그인 성공 {loginUser.DisplayName}");
            OnLoginSuccess?.Invoke(loginUser);
        });
    }

    void SignInAnonymously()
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
        });
    }

    void SetGuestName()
    {
        if (!string.IsNullOrEmpty(localDisplayName)) return;
        string randomSuffix = UnityEngine.Random.Range(10000, 99999).ToString();
        string newName = $"Guest_{randomSuffix}";

        localDisplayName = newName;
        UpdateProfile(newName);
    }

    void UpdateProfile(string newNickname)
    {
        localDisplayName = newNickname;

        UserProfile profile = new UserProfile { DisplayName = newNickname };

        firebaseUser.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("닉네임 설정 실패");
                return;
            }
            else
            {
                Debug.Log($"닉네임 업데이트 완료: {newNickname}");
            }
        });
    }

    public void SignOut()
    {
        firebaseAuth.SignOut();
        SignInAnonymously();
    }

    void OnApplicationQuit()
    {
        if (firebaseAuth != null)
        {
            firebaseAuth.SignOut();
            Debug.Log("[종료] 자동 로그아웃");
        }
    }

    void OnDestroy()
    {
        if (firebaseAuth != null)
        {
            firebaseAuth.StateChanged -= AuthStateChanged;
        }
    }
}
