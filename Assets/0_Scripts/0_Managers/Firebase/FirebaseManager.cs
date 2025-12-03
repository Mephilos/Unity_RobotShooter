using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections.Generic;


[Serializable]
public class UserScoreData
{
    public string userName;
    public int score;
    public float time;
    public float acc;

    public UserScoreData(string userName, int score, float time, float acc)
    {
        this.userName = userName;
        this.score = score;
        this.time = time;
        this.acc = acc;
    }
}

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;
    DatabaseReference databaseReference;

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
        AuthManager.Instance.OnLoginSuccess += OnLoginHandler;
        AuthManager.Instance.OnLogout += OnLogoutHandler;
    }

    void OnLoginHandler(Firebase.Auth.FirebaseUser firebaseUser)
    {
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        LoadMyData();
    }
    void OnLogoutHandler()
    {
        dataInit();
        isDataLoad = false;
    }
    void LoadMyData()
    {
        string userId = AuthManager.Instance.UserId;
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
                    dataInit();
                }
                isDataLoad = true;
            }
        });
    }
    public void RenewScore(int currentScore, float currentTime, float currentAcc)
    {
        string userId = AuthManager.Instance.UserId;
        string userName = AuthManager.Instance.DisplayName;

        if (string.IsNullOrEmpty(userId)) return;

        bool isNewScore = false;

        if (currentScore > BestScore)
        {
            BestScore = currentScore;
            BestTime = currentTime;
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
            UserScoreData userScoreData = new UserScoreData(userName, BestScore, BestTime, BestAcc);
            string json = JsonUtility.ToJson(userScoreData);

            databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json);
            Debug.Log($"서버에 신기록 갱신 완료({BestScore})");
        }
    }

    public void LoadLeaderboardData(Action<List<UserScoreData>> onLoad)
    {
        databaseReference.Child("users").OrderByChild("score").LimitToLast(10).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("리더보드 실패");
                return;
            }

            List<UserScoreData> rankList = new List<UserScoreData>();
            DataSnapshot dataSnapshot = task.Result;

            foreach (DataSnapshot data in dataSnapshot.Children)
            {
                IDictionary<string, object> rankData = (IDictionary<string, object>)data.Value;

                string uName = rankData.ContainsKey("userName") ? rankData["userName"].ToString() : "UnknownPlayer";
                int uScore = rankData.ContainsKey("score") ? Convert.ToInt32(rankData["score"]) : 0;
                float uTime = rankData.ContainsKey("time") ? Convert.ToSingle(rankData["time"]) : 0f;
                float uAcc = rankData.ContainsKey("acc") ? Convert.ToSingle(rankData["acc"]) : 0f;

                rankList.Add(new UserScoreData(uName, uScore, uTime, uAcc));
            }

            rankList.Reverse();

            onLoad?.Invoke(rankList);
            Debug.Log("리더보드 로드 완료");
        });
    }
    void dataInit()
    {
        BestScore = 0;
        BestTime = 9999f;
        BestAcc = 0f;
    }
}
