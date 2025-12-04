using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardHandler : MonoBehaviour
{
    [SerializeField] GameObject leaderboardRowPrefab;
    [SerializeField] Transform contentParent;
    [SerializeField] TMP_Text pageText;
    [SerializeField] Button prevButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button myRankButton;
    [SerializeField] int rowPerPage = 11;

    List<UserScoreData> allData = new List<UserScoreData>();
    int totalCount = 0;
    int currentPage = 1;
    int maxPage = 1;

    void Start()
    {
        prevButton.onClick.AddListener(PrevPage);
        nextButton.onClick.AddListener(NextPage);
        myRankButton.onClick.AddListener(JumpMyRank);

        FirebaseManager.Instance.LoadLeaderboardData(OnDataLoad);
    }
    void OnEnable()
    {
        FirebaseManager.Instance.LoadLeaderboardData(OnDataLoad);
    }

    void OnDataLoad(List<UserScoreData> userScoreDatas)
    {
        allData = userScoreDatas;
        totalCount = allData.Count;

        maxPage = Mathf.CeilToInt((float)totalCount / rowPerPage);
        if (1 < maxPage)
        {
            maxPage = 1;
        }

        JumpMyRank();
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

            leaderboardRow.SetData(rank, userScoreData.userName, userScoreData.score, userScoreData.acc, percent, isMyName);
        }
        pageText.text = $"{currentPage} / {maxPage}";

        prevButton.interactable = (currentPage > 1);
        nextButton.interactable = (currentPage < maxPage);
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
