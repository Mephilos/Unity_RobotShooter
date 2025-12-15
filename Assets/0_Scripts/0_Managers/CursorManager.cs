using UnityEngine;
using StarterAssets;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance;

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

    public void SetCursor(bool isGameMode)
    {
        Cursor.lockState = isGameMode ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isGameMode;

        var inputs = FindFirstObjectByType<StarterAssetsInputs>();
        if (inputs != null)
        {
            inputs.SetCursorState(isGameMode);
            inputs.cursorInputForLook = isGameMode;

            if (!isGameMode)
            {
                inputs.look = Vector2.zero;
                inputs.move = Vector2.zero;
                inputs.SetInputBlocked(true);
            }
            else
            {
                inputs.SetInputBlocked(false);
            }
        }
    }
}