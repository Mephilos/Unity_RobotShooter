using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using StarterAssets;
public class OptionsHandler : MonoBehaviour
{
    [SerializeField] TMP_Dropdown gameContext;
    [SerializeField] TMP_InputField sensInput;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider sfxSlider;

    private readonly float[] sensMultipliers = new float[]
    {
        1f, // cs2, apex
        1f / 3.181818f, // valo
        3.333f // overwatch
    };

    float currentSens = 1.0f;

    void Start()
    {
        InitSoundUI();
        InitSensUI();
    }

    void InitSensUI()
    {
        gameContext.ClearOptions();
        gameContext.ClearOptions();
        gameContext.AddOptions(new List<string> { "standard / cs2 / apex", "valorant", "overwatch" });

        currentSens = PlayerPrefs.GetFloat("BaseSens", 1.0f);

        gameContext.value = 0;
        UpdateInputDisplay(0);

        gameContext.onValueChanged.AddListener(OnGameChanged);
        sensInput.onEndEdit.AddListener(OnSensChanged);
    }

    void OnGameChanged(int index)
    {
        UpdateInputDisplay(index);
    }

    void OnSensChanged(string value)
    {
        if (float.TryParse(value, out float inputSens))
        {
            int selectedGame = gameContext.value;
            float multiplier = sensMultipliers[selectedGame];

            // 입력값(게임 감도) 기준 감도로 역산
            currentSens = inputSens / multiplier;


            ApplySensitivity();
        }
    }

    void UpdateInputDisplay(int gameContextIndex)
    {
        float multiplier = sensMultipliers[gameContextIndex];
        float displayValue = currentSens * multiplier;

        sensInput.text = displayValue.ToString("F3");
    }

    void ApplySensitivity()
    {

        PlayerPrefs.SetFloat("BaseSens", currentSens);

        float finalSens = currentSens * Constants.MOUSE_SENS_MULTIPLIER;
        PlayerPrefs.SetFloat("MouseSens", finalSens);
        PlayerPrefs.Save();
        var player = FindFirstObjectByType<FirstPersonController>();


        player.RotationSpeed = PlayerPrefs.GetFloat("MouseSens", Constants.MOUSE_SENS_MULTIPLIER);
        var weapon = player.GetComponentInChildren<ActiveWeapon>();
        weapon.UpdateSensitivity(finalSens);

        Debug.Log($"감도 저장 전: {currentSens} / 후: {finalSens}");
    }

    void InitSoundUI()
    {
        if (SoundManager.Instance == null) return;

        masterSlider.value = SoundManager.Instance.MasterVolume;
        bgmSlider.value = SoundManager.Instance.BgmVolume;
        sfxSlider.value = SoundManager.Instance.SfxVolume;

        masterSlider.onValueChanged.AddListener(SoundManager.Instance.SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBgmVolume);
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSfxVolume);
    }
}
