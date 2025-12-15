using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;


public class PlayMenuHandler : MenuHandler
{
    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject optionPanel;
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
        GameManager.Instance.OnPauseToggle += PausePanelPopup;
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
        GameManager.Instance.OnPauseToggle -= PausePanelPopup;
    }

    void OnPauseInput(InputAction.CallbackContext callback)
    {
        GameManager.Instance?.PauseToggle();
        optionPanel.SetActive(false);

    }

    void PausePanelPopup(bool isPause)
    {
        pausePanel.SetActive(isPause);
    }
}
