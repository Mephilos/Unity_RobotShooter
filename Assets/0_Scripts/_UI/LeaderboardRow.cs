using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] TMP_Text rankText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text timeText;
    [SerializeField] TMP_Text accText;
    [SerializeField] TMP_Text percentText;
    [SerializeField] Image rowBackground;

    [SerializeField] Color myRankColor = new Color(1f, 1f, 0f, .3f);
    [SerializeField] Color normalColor = new Color(1f, 1f, 1f, 0f);

    public void SetData(int rank, string name, int score, float time, float acc, float percent, bool isMy)
    {
        rankText.text = rank.ToString();
        nameText.text = name;
        scoreText.text = score.ToString("N0");

        TimeSpan ts = TimeSpan.FromSeconds(time);
        timeText.text = string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds);

        accText.text = $"{acc:F1}%";
        percentText.text = $"{percent:F1}%";

        if (isMy)
        {
            rowBackground.color = myRankColor;
        }
        else
            rowBackground.color = normalColor;
    }
}
