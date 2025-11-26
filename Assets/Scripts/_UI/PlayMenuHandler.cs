using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;


public class PlayMenuHandler : MenuHandler
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] InputActionReference pauseDetec;
    StarterAssetsInputs playerInputs;
    void Awake()
    {
        Time.timeScale = 1.0f;
    }
    protected override void Start()
    {
        base.Start();
        playerInputs = FindFirstObjectByType<StarterAssetsInputs>();
        GameManager.instance.OnPauseToggle += PausePanelPopup;
    }

    void OnDisable()
    {
        pauseDetec.action.performed -= OnPauseInput;
        pauseDetec.action.Disable();
    }
    void OnEnable()
    {
        pauseDetec.action.Enable();
        pauseDetec.action.performed += OnPauseInput;
    }


    void OnDestroy()
    {
        GameManager.instance.OnPauseToggle -= PausePanelPopup;
    }

    void OnPauseInput(InputAction.CallbackContext callback)
    {
        GameManager.instance?.PauseToggle();
    }

    void PausePanelPopup(bool isPause)
    {
        pausePanel.SetActive(isPause);

        if (playerInputs != null)
        {
            playerInputs.SetInputBlocked(isPause);
        }
    }
}
