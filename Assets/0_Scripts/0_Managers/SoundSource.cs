using System.Collections;
using UnityEditor.Timeline.Actions;
using UnityEngine;

public class SoundSource : MonoBehaviour
{
    AudioSource audioSource;
    float disableTime;
    bool isPlay = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isPlay)
        {
            if (Time.time >= disableTime)
            {
                Release();
            }
        }
    }

    public void Play(AudioClip audioClip, float volume, float pitch)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = 1f;
        audioSource.maxDistance = 20f;

        audioSource.Play();

        disableTime = Time.time + audioClip.length + .1f;
        isPlay = true;
    }

    void Release()
    {
        isPlay = false;
        audioSource.Stop();
        audioSource.clip = null;

        PoolManager.Instance.Release(gameObject);
    }

    void OnDisable()
    {
        isPlay = false;
    }
}
