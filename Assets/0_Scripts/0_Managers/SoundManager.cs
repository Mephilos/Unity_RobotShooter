using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] GameObject soundSourcePrefab;
    [SerializeField] AudioSource bgmSource;

    [Range(0f, 1f)]
    public float MasterVolume = 1f;
    [Range(0f, 1f)]
    public float SfxVolume = 1f;
    [Range(0f, 1f)]
    public float BgmVolume = 1f;

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

    public void PlaySFX(AudioClip audioClip, Vector3 position, float pitch = 1f)
    {
        GameObject obj = PoolManager.Instance.Get(soundSourcePrefab, position, Quaternion.identity);

        if (obj.TryGetComponent<SoundSource>(out SoundSource source))
        {
            source.Play(audioClip, MasterVolume * SfxVolume, pitch);
        }
    }
    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmSource == null) return;

        bgmSource.clip = bgmClip;
        bgmSource.loop = true;
        bgmSource.volume = MasterVolume * BgmVolume;
        bgmSource.Play();
    }
    public void PlayUISFX(AudioClip audioClip, float pitch = 1f)
    {
        PlaySFX(audioClip, Camera.main.transform.position, pitch);
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        UpdateBgmVolume();
        PlayerPrefs.SetFloat("MasterVol", value);

    }
    public void SetBgmVolume(float value)
    {
        BgmVolume = value;
        UpdateBgmVolume();
        PlayerPrefs.SetFloat("BgmVol", value);
    }
    public void SetSfxVolume(float value)
    {
        SfxVolume = value;
        PlayerPrefs.SetFloat("SfxVol", value);
    }

    void UpdateBgmVolume()
    {
        bgmSource.volume = MasterVolume * BgmVolume;
    }

    void LoadVolume()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVol", 1f);
        BgmVolume = PlayerPrefs.GetFloat("BgmVol", 1f);
        SfxVolume = PlayerPrefs.GetFloat("SfxVol", 1f);
        UpdateBgmVolume();
    }
}
