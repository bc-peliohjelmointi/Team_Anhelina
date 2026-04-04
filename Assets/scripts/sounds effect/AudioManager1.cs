using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager1 : MonoBehaviour
{
    public static AudioManager1 Instance;

    public AudioMixer audioMixer;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
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

    void Start()
    {
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = 1f;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float value = Mathf.Lerp(startVolume, 0.0001f, time / duration);
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);
            yield return null;
        }
    }
}