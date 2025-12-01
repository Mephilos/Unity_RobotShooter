using UnityEngine;
using System.Text;
using System.IO;
using System.Collections;
using System;
using Unity.Profiling;

public class OptimizationMeasurement : MonoBehaviour
{
    [SerializeField] bool saveToCSV = true;
    [SerializeField] bool before = true;
    [SerializeField] float recordInterval = 0.5f;


    string savePath;
    StringBuilder csvContent = new StringBuilder();
    float deltaTime = 0.0f;
    float timer = 0.0f;

    ProfilerRecorder profilerRecorder;

    void OnEnable()
    {
        profilerRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC in Frame");
    }
    void OnDisable()
    {
        profilerRecorder.Dispose();
    }
    void Start()
    {
        csvContent.AppendLine("Time,FPS, Memory, GC Alloc, GC Count");
        if (before)
        {
            savePath = Application.dataPath + "/../OptimizationLogs/Before";
        }
        else
        {
            savePath = Application.dataPath + "/../OptimizationLogs/After";
        }

        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (saveToCSV)
        {
            timer += Time.deltaTime;
            if (timer > -recordInterval)
            {
                RecordData();
                timer = 0f;
            }
        }
    }

    void RecordData()
    {
        float fps = 1.0f / deltaTime;
        float totalMem = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024f / 1024f;

        float gcAlloc = profilerRecorder.LastValue / 1024f;
        int gcCount = GC.CollectionCount(0);

        csvContent.AppendLine($"{Time.time:F1},{fps:F1},{totalMem:F1},{gcAlloc:F1},{gcCount}");
    }

    void OnApplicationQuit()
    {
        if (saveToCSV)
        {
            string fileName = $"Performance_{System.DateTime.Now:MMdd_HHmm_ss}.csv";
            string fullPath = Path.Combine(savePath, fileName);

            File.WriteAllText(fullPath, csvContent.ToString());
            Debug.Log($"최적화 정보 저장 : {fullPath}");
        }
    }
}