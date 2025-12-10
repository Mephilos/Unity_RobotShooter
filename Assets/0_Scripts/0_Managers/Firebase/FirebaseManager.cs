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

    public int BestScore { get; private set; } = 0;
    public float BestTime { get; private set; } = 9999f;
    public float BestAcc { get; private set; } = 0f;

    DatabaseReference databaseReference;
    bool isDataLoad = false;

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
        if (isDataLoad) return;

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
                    if (data.ContainsKey("acc")) BestAcc = Convert.ToSingle(data["acc"]);

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
    public void StageScoreSave(int stageIndex, int currentScore, float currentTime, float currentAcc, Action<bool, int> OnComplete)
    {
        string userId = AuthManager.Instance.UserId;
        string userName = AuthManager.Instance.DisplayName;

        if (string.IsNullOrEmpty(userId)) return;

        DatabaseReference stageDataRef = databaseReference.Child("users").Child(userId).Child("stages").Child(stageIndex.ToString());
        stageDataRef.GetValueAsync().ContinueWithOnMainThread(task =>
        {
            bool isNewScore = false;
            if (task.IsFaulted)
            {
                Debug.LogError("기록조회 실패");
                OnComplete?.Invoke(false, 0);
                return;
            }

            DataSnapshot snapshot = task.Result;
            int dbBestScore = 0;
            float dbBestTime = 9999f;

            if (snapshot.Exists && snapshot.HasChildren)
            {
                IDictionary<string, object> data = (IDictionary<string, object>)snapshot.Value;
                if (data.ContainsKey("score")) dbBestScore = Convert.ToInt32(data["score"]);
                if (data.ContainsKey("time")) dbBestTime = Convert.ToSingle(data["time"]);
            }

            if (currentScore > dbBestScore)
            {
                isNewScore = true;
            }
            else if (currentScore == dbBestScore && currentTime < dbBestTime)
            {
                isNewScore = true;
            }

            if (isNewScore)
            {
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    ["userName"] = userName,
                    ["score"] = currentScore,
                    ["time"] = currentTime,
                    ["acc"] = currentAcc
                };
                databaseReference.Child("users").Child(userId).Child("userName").SetValueAsync(userName);

                stageDataRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log($"[신기록 달성] 스테이지 {stageIndex} | 점수: {currentScore}");
                        TryUpdateTotalScore();
                        OnComplete?.Invoke(true, currentScore);
                    }
                });
            }
            else
            {
                OnComplete?.Invoke(false, dbBestScore);
            }
        });
    }

    void TryUpdateTotalScore()
    {
        string userId = AuthManager.Instance.UserId;
        if (string.IsNullOrEmpty(userId)) return;

        databaseReference.Child("users").Child(userId).Child("stages").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists) return;

            DataSnapshot stagesSnapshot = task.Result;

            if (stagesSnapshot.ChildrenCount < Constants.TOTAL_STAGE_COUNT)
            {
                Debug.Log($"합산X: {stagesSnapshot.ChildrenCount} / {Constants.TOTAL_STAGE_COUNT}");
                return;
            }

            List<StagesScore> dbScore = new List<StagesScore>();
            // int totalScore = 0;
            // float totalTime = 0;
            // float totalAccSum = 0;
            // int count = 0;

            foreach (DataSnapshot stage in stagesSnapshot.Children)
            {
                IDictionary<string, object> value = (IDictionary<string, object>)stage.Value;
                StagesScore scoreDate = new StagesScore
                {
                    score = Convert.ToInt32(value["score"]),
                    time = Convert.ToSingle(value["time"]),
                    acc = Convert.ToSingle(value["acc"])
                };
                dbScore.Add(scoreDate);
            }

            var stats = ScoreManager.CalculateStats(dbScore);

            Debug.Log($"스테이지 올 클리어 합산 결과> 점수: {stats.totalScore}/ 시간: {stats.totalTime}/ 정확도: {stats.avgAcc}");
            RenewScore(stats.totalScore, stats.totalTime, stats.avgAcc);
        });
    }

    void RenewScore(int currentScore, float currentTime, float currentAcc)
    {
        string userId = AuthManager.Instance.UserId;
        string userName = AuthManager.Instance.DisplayName;

        if (string.IsNullOrEmpty(userId)) return;

        bool isNewScore = false;

        if (currentScore > BestScore)
        {
            isNewScore = true;
        }

        else if (currentScore == BestScore && currentTime < BestTime)
        {
            isNewScore = true;
        }

        if (isNewScore)
        {
            Dictionary<string, object> scoreUpdate = new Dictionary<string, object>
            {
                ["userName"] = userName,
                ["score"] = currentScore,
                ["time"] = currentTime,
                ["acc"] = currentAcc
            };
            databaseReference.Child("users").Child(userId).UpdateChildrenAsync(scoreUpdate);
            Debug.Log($"종합기록 갱신 완료 서버 등록({BestScore})");
        }
    }

    public void LoadLeaderboardData(int stageIndex, Action<List<UserScoreData>> onLoad)
    {
        string stageScorePath = (stageIndex == 0) ? "score" : $"stages/{stageIndex}/score";

        databaseReference.Child("users").OrderByChild(stageScorePath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("리더보드 로드 실패" + task.Exception);
                onLoad?.Invoke(new List<UserScoreData>());
                return;
            }

            List<UserScoreData> rankList = new List<UserScoreData>();
            DataSnapshot dataSnapshot = task.Result;

            foreach (DataSnapshot data in dataSnapshot.Children)
            {

                int uScore = 0;
                float uTime = 0f;
                float uAcc = 0f;
                string uName = "UnknownPlayer";

                if (data.HasChild("userName")) uName = data.Child("userName").Value.ToString();

                if (stageIndex == 0)
                {
                    if (!data.HasChild("score")) continue;

                    uScore = Convert.ToInt32(data.Child("score").Value);
                    if (data.HasChild("time")) uTime = Convert.ToSingle(data.Child("time").Value);
                    if (data.HasChild("acc")) uAcc = Convert.ToSingle(data.Child("acc").Value);
                }
                else
                {
                    if (!data.HasChild("stages") || !data.Child("stages").Child(stageIndex.ToString()).Exists) continue;

                    DataSnapshot stageSnap = data.Child("stages").Child(stageIndex.ToString());
                    if (stageSnap.HasChild("score")) uScore = Convert.ToInt32(stageSnap.Child("score").Value);
                    if (stageSnap.HasChild("time")) uTime = Convert.ToSingle(stageSnap.Child("time").Value);
                    if (stageSnap.HasChild("acc")) uAcc = Convert.ToSingle(stageSnap.Child("acc").Value);
                }
                rankList.Add(new UserScoreData(uName, uScore, uTime, uAcc));
            }
            rankList.Reverse();

            onLoad?.Invoke(rankList);
            Debug.Log($"리더보드 로드 완료 총: {rankList.Count}");
        });
    }
    void dataInit()
    {
        BestScore = 0;
        BestTime = 9999f;
        BestAcc = 0f;
    }
}
