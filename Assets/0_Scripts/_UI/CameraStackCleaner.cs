using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraStackCleaner : MonoBehaviour
{
    void Awake()
    {
        var cameraData = GetComponent<UniversalAdditionalCameraData>();
        if (cameraData != null)
        {
            int removedCount = cameraData.cameraStack.RemoveAll(c => c == null);
        }
    }
}