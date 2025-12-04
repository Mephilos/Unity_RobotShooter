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
        bool isNewAcc = false;

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
            isNewAcc = true;
        }

        DatabaseReference userDataRef = databaseReference.Child("users").Child(userId);

        if (isNewScore)
        {
            Dictionary<string, object> scoreUpdate = new Dictionary<string, object>
            {
                ["userName"] = userName,
                ["score"] = BestScore,
                ["time"] = BestTime
            };
            userDataRef.UpdateChildrenAsync(scoreUpdate);
            Debug.Log($"신기록 갱신 완료 서버 등록({BestScore})");
        }

        if (isNewAcc)
        {
            Dictionary<string, object> accUpdate = new Dictionary<string, object>
            {
                ["userName"] = userName,
                ["acc"] = BestAcc
            };
            userDataRef.UpdateChildrenAsync(accUpdate);
            Debug.Log($"정확도 갱신 완료 서버 등록({BestAcc})");
        }
    }

    public void StageRecordSave(int stageIndex, int score, float time, float acc)
    {
        string userId = AuthManager.Instance.UserId;
        if (string.IsNullOrEmpty(userId)) return;

        DatabaseReference stageDataRef = databaseReference.Child("users").Child(userId).Child("stages").Child(stageIndex.ToString());

        Dictionary<string, object> update = new Dictionary<string, object>()
        {
            ["score"] = score,
            ["time"] = time,
            ["acc"] = acc
        };

        stageDataRef.UpdateChildrenAsync(update).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                TryUpdateTotalScore();
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

            int totalScore = 0;
            float totalTime = 0;
            float totalAccSum = 0;
            int count = 0;

            foreach (DataSnapshot stage in stagesSnapshot.Children)
            {
                IDictionary<string, object> value = (IDictionary<string, object>)stage.Value;
                totalScore += Convert.ToInt32(value["score"]);
                totalTime += Convert.ToSingle(value["time"]);
                totalAccSum += Convert.ToSingle(value["acc"]);
                count++;
            }

            int finalTotalScore = totalScore;
            float finalTotalTime = totalTime;
            float finalAvgAcc = (count > 0) ? (totalAccSum / count) : 0f;

            Debug.Log($"스테이지 올 클리어 합산 결과> 점수: {finalTotalScore}/ 시간: {finalTotalTime}/ 정확도: {finalAvgAcc}");
            RenewScore(finalTotalScore, finalTotalTime, finalAvgAcc);
        });
    }
    public void LoadLeaderboardData(int stageIndex, Action<List<UserScoreData>> onLoad)
    {
        string stageScorePath = (stageIndex == 0) ? "score" : $"stages/{stageIndex}/score";

        databaseReference.Child("users").OrderByChild(stageScorePath).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("리더보드 로드 실패" + task.Exception);
                return;
            }

            List<UserScoreData> rankList = new List<UserScoreData>();
            DataSnapshot dataSnapshot = task.Result;

            foreach (DataSnapshot data in dataSnapshot.Children)
            {
                string uName = "UnknownPlayer";
                if (data.HasChild("userName"))
                {
                    uName = data.Child("userName").Value.ToString();
                }

                int uScore = 0;
                float uTime = 0f;
                float uAcc = 0f;

                if (stageIndex == 0)
                {

                    if (!data.HasChild("score")) continue;

                    uScore = Convert.ToInt32(data.Child("score").Value);
                    if (data.HasChild("time")) uTime = Convert.ToSingle(data.Child("time").Value);
                    if (data.HasChild("acc")) uAcc = Convert.ToSingle(data.Child("acc").Value);
                }
                else
                {
                    if (!data.HasChild("stages")) continue;

                    DataSnapshot stageSnap = data.Child("stages").Child(stageIndex.ToString());
                    if (!stageSnap.Exists) continue;

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
