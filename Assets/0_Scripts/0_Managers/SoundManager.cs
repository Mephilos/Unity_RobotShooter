using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] GameObject soundSourcePrefab;

    [Range(0f, 1f)]
    public float MasterVolume = 1f;
    [Range(0f, 1f)]
    public float SfxVolume = 1f;

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

    public void PlayUISFX(AudioClip audioClip, float pitch = 1f)
    {
        PlaySFX(audioClip, Camera.main.transform.position, pitch);
    }
}
