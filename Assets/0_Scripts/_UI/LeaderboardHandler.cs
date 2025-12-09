using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardHandler : MonoBehaviour
{
    [SerializeField] GameObject leaderboardRowPrefab;
    [SerializeField] Transform contentParent;
    [SerializeField] TMP_Text pageText;
    [SerializeField] GameObject loadingObject;

    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button myRankButton;

    [SerializeField] Button totalScoreButton;
    [SerializeField] Button[] stageScoreButton;

    [SerializeField] int rowPerPage = 11;

    List<UserScoreData> allData = new List<UserScoreData>();
    int totalCount = 0;
    int currentPage = 1;
    int maxPage = 1;

    int currentScoreView = 0;
    bool isLoading = false;

    void Start()
    {
        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);
        myRankButton.onClick.AddListener(JumpMyRank);

        totalScoreButton.onClick.AddListener(() => ChangeScoreView(0));

        for (int i = 0; i < stageScoreButton.Length; i++)
        {
            int stageNum = i + 1;
            int index = i;
            stageScoreButton[index].onClick.AddListener(() => ChangeScoreView(stageNum));
        }
    }

    void OnEnable()
    {
        ChangeScoreView(currentScoreView);
    }

    void ChangeScoreView(int stage)
    {
        if (isLoading) return;

        currentScoreView = stage;
        currentPage = 1;

        SetLoadingState(true);

        FirebaseManager.Instance.LoadLeaderboardData(currentScoreView, OnDataLoad);
    }

    void OnDataLoad(List<UserScoreData> userScoreDatas)
    {
        if (this == null) return; // 로드중에 파괴되면 중단

        SetLoadingState(false);

        allData = userScoreDatas;
        totalCount = allData.Count;

        maxPage = Mathf.CeilToInt((float)totalCount / rowPerPage);
        if (1 > maxPage)
        {
            maxPage = 1;
        }

        JumpMyRank();
    }

    void SetLoadingState(bool loading)
    {
        isLoading = loading;

        loadingObject.SetActive(loading);

        totalScoreButton.interactable = !loading;
        foreach (var button in stageScoreButton)
        {
            button.interactable = !loading;
        }
        prevButton.interactable = !loading;
        nextButton.interactable = !loading;
    }

    void RefreshUI()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        int startIndex = (currentPage - 1) * rowPerPage;
        int endIndex = Mathf.Min(startIndex + rowPerPage, totalCount);

        string myName = AuthManager.Instance.DisplayName;

        for (int i = startIndex; i < endIndex; i++)
        {
            UserScoreData userScoreData = allData[i];

            GameObject row = Instantiate(leaderboardRowPrefab, contentParent);
            LeaderboardRow leaderboardRow = row.GetComponent<LeaderboardRow>();

            int rank = i + 1;

            float percent = (float)(totalCount - i) / totalCount * 100f;

            bool isMyName = (userScoreData.userName == myName);

            leaderboardRow.SetData(rank, userScoreData.userName, userScoreData.score, userScoreData.time, userScoreData.acc, percent, isMyName);
        }
        pageText.text = $"{currentPage} / {maxPage}";

        prevButton.interactable = (currentPage > 1);
        nextButton.interactable = (currentPage < maxPage);
    }

    public void LoadStageLeaderboard(int stageIndex)
    {
        ChangeScoreView(stageIndex);
    }

    public void PrevPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            RefreshUI();
        }
    }

    public void NextPage()
    {
        if (currentPage < maxPage)
        {
            currentPage++;
            RefreshUI();
        }
    }

    public void JumpMyRank()
    {
        string myName = AuthManager.Instance.DisplayName;
        int myIndex = allData.FindIndex(x => x.userName == myName);

        if (myIndex != -1) // 기록이 존재하면 가고
        {
            currentPage = (myIndex / rowPerPage) + 1;
        }
        else // 없으면 그냥 1페이지
        {
            currentPage = 1;
        }

        RefreshUI();
    }
}
