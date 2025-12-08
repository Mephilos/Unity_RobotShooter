using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;
public class StageClearTimeData
{
    public int stageIndex;
    public float limitTime; // 클리어 목표 시간
    public int scoreTime;   // 시간 점수 기준
}
public class CSVManager : MonoBehaviour
{
    public static CSVManager Instance;

    Dictionary<int, StageClearTimeData> clearDataDict = new Dictionary<int, StageClearTimeData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCSVData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadCSVData()
    {
        TextAsset csvData = Resources.Load<TextAsset>("TimeData");

        if (csvData == null)
        {
            Debug.LogError($"{gameObject} csv파일 확인 요망");
            return;
        }

        string[] lines = csvData.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] data = lines[i].Split(',');

            if (data.Length >= 3)
            {
                try
                {
                    StageClearTimeData stageClearTimeData = new StageClearTimeData();
                    stageClearTimeData.stageIndex = int.Parse(data[0]);
                    stageClearTimeData.limitTime = float.Parse(data[1]);
                    stageClearTimeData.scoreTime = int.Parse(data[2]);

                    clearDataDict.Add(stageClearTimeData.stageIndex, stageClearTimeData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{gameObject} CSV 파싱 {i}번째: {e.Message}");
                }
            }
        }
    }

    public StageClearTimeData GetStageClearTimeData(int stageIndex)
    {
        if (clearDataDict.ContainsKey(stageIndex))
        {
            return clearDataDict[stageIndex];
        }

        Debug.LogWarning($"{gameObject} 스테이지{stageIndex}의 클리어타임 데이터가 없습니다.");
        return null;
    }
}
