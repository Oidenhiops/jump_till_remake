using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioMixer audioMixer;
    public AudioSource bgmSource;
    public SoundsDBSO soundsDB;
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
    public void PlayASound(AudioClip audioClip, bool needRandomPitch = false)
    {
        AudioSource audioBox = Instantiate(GameData.Instance.utils.prefabs["AudioBox"]).GetComponent<AudioSource>();
        audioBox.clip = audioClip;
        audioBox.pitch = needRandomPitch ? UnityEngine.Random.Range(0.85f, 1.25f) : 1;
        audioBox.Play();
        StartCoroutine(DestroyAudioBox(audioBox.gameObject, audioClip.length));
    }
    public async Awaitable FadeIn()
    {
        try
        {
            if (!GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute)
            {
                float targetDecibels = 20 * Mathf.Log10(GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.volumes[TypeSound.MASTER] / 100);
                float currentVolume;

                if (!audioMixer.GetFloat(TypeSound.MASTER.ToString(), out currentVolume))
                {
                    currentVolume = -80f;
                }
                float duration = 1f;
                float elapsed = 0f;
                float updateRate = 1f / 60f;
                while (elapsed < duration)
                {
                    elapsed += updateRate;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float newVolume = Mathf.Lerp(currentVolume, targetDecibels, t);
                    audioMixer.SetFloat(TypeSound.MASTER.ToString(), newVolume);
                    await Task.Delay(TimeSpan.FromSeconds(updateRate));
                }
                audioMixer.SetFloat(TypeSound.MASTER.ToString(), targetDecibels);
                await Awaitable.NextFrameAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public async Awaitable FadeOut()
    {
        try
        {
            if (!GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute)
            {
                float currentVolume;
                if (!audioMixer.GetFloat(TypeSound.MASTER.ToString(), out currentVolume))
                {
                    currentVolume = 20 * Mathf.Log10(GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.volumes[TypeSound.MASTER] / 100);
                }
                float targetVolume = -80f;
                float duration = 1f;
                float elapsed = 0f;
                float updateRate = 1f / 60f;
                while (elapsed < duration)
                {
                    elapsed += updateRate;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float newVolume = Mathf.Lerp(currentVolume, targetVolume, t);
                    audioMixer.SetFloat(TypeSound.MASTER.ToString(), newVolume);
                    await Task.Delay(TimeSpan.FromSeconds(updateRate));
                }
                audioMixer.SetFloat(TypeSound.MASTER.ToString(), targetVolume);
                await Awaitable.NextFrameAsync();
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    public AudioClip GetAudioClip(SoundsDBSO.TypeSound typeSound, string soundId)
    {
        if (soundsDB.sounds.TryGetValue(typeSound, out Dictionary<string, AudioClip[]> sounds))
        {
            if (sounds.TryGetValue(soundId, out AudioClip[] clips))
            {
                return clips[UnityEngine.Random.Range(0, clips.Length - 1)];
            }
            return null;
        }
        return null;
    }
    public void SetAudioMixerData(bool whitOutmMaster = false)
    {
        foreach(KeyValuePair<TypeSound, float> sounds in GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.volumes)
        {
            if (!whitOutmMaster || whitOutmMaster && sounds.Key != TypeSound.MASTER)
            {
                float volume = GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.volumes[sounds.Key] != 0 ?
                    20 * Mathf.Log10(GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.volumes[sounds.Key] / 100) : -80;
                audioMixer.SetFloat(sounds.Key.ToString(), volume);
            }
        }
        if (GameData.Instance.systemDataInfo.configurationsInfo.soundConfiguration.isMute) audioMixer.SetFloat(TypeSound.MASTER.ToString(), -80);
        GameData.Instance.SaveSystemData();
    }
    IEnumerator DestroyAudioBox(GameObject audioBox, float timeToDestroy)
    {
        yield return new WaitForSecondsRealtime(timeToDestroy);
        Destroy(audioBox);
    }
    public enum TypeSound
    {
        MASTER = 0,
        BGM = 1,
        SFX = 2
    }    
}
