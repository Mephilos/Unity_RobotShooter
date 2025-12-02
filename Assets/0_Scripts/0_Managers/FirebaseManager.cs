using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Serializable]
public class UserScoreData
{
    public int score;
    public float time;
    public float acc;

    public UserScoreData(int score, float time, float acc)
    {
        this.score = score;
        this.time = time;
        this.acc = acc;
    }
}

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    FirebaseAuth firebaseAuth;
    DatabaseReference databaseReference;

    string userId;

    public int BestScore { get; private set; } = 0;
    public float BestTime { get; private set; } = 9999f;
    public float BestAcc { get; private set; } = 0f;

    public bool isDataLoad = false;

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

    void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                firebaseAuth = FirebaseAuth.DefaultInstance;
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                SignInAnonymously();
            }
            else
            {
                Debug.LogError($"파이어베이스 연결 실패: {dependencyStatus}");
            }
        });
    }

    void SignInAnonymously()
    {
        firebaseAuth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("익명 로그인 실패");
                return;
            }

            FirebaseUser newUser = task.Result.User;
            userId = newUser.UserId;
            Debug.Log($"로그인 완료 ID: {userId}");

            LoadMyData();
        });
    }
    void LoadMyData()
    {
        if (string.IsNullOrEmpty(userId)) return;

        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot dataSnapshot = task.Result;
                if (dataSnapshot.Exists && dataSnapshot.HasChildren)
                {
                    IDictionary<string, object> data = (IDictionary<string, object>)dataSnapshot.Value;
                    if (data.ContainsKey("score")) BestScore = Convert.ToInt32(data["score"]);
                    if (data.ContainsKey("time")) BestTime = Convert.ToSingle(data["time"]);
                    if (data.ContainsKey("score")) BestAcc = Convert.ToSingle(data["acc"]);

                    Debug.Log($"기록 로드 완료| 점수: {BestScore}, 시간: {BestTime}, 정확도:{BestAcc}");
                }
                else
                {
                    Debug.Log("신규 유저");
                }
                isDataLoad = true;
            }
        });
    }
    public void RenewScore(int currentScore, float currentTime, float currentAcc)
    {
        if (string.IsNullOrEmpty(userId)) return;

        bool isNewScore = false;

        if (currentScore > BestScore)
        {
            BestScore = currentScore;
            isNewScore = true;
        }

        else if (currentScore == BestScore && currentTime < BestTime)
        {
            BestTime = currentTime;
            isNewScore = true;
        }

        if (currentAcc > BestAcc)
        {
            BestAcc = currentAcc;
        }

        if (isNewScore)
        {
            UserScoreData userScoreData = new UserScoreData(BestScore, BestTime, BestAcc);
            string json = JsonUtility.ToJson(userScoreData);

            databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json);
            Debug.Log("서버에 신기록 갱신 완료");
        }
    }
}
