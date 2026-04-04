using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixerController : MonoBehaviour
{
    public AudioMixer audioMixer; // main audio mixer
    public Slider musicSlider;    // UI slider for music
    public Slider sfxSlider;      // UI slider for SFX

    void Start()
    {
        // load saved volume 
        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // set slider values
        musicSlider.value = music;
        sfxSlider.value = sfx;

        // apply volume to mixer
        SetMusicVolume(music);
        SetSFXVolume(sfx);
    }

    public void SetMusicVolume(float value)
    {
        // convert linear into dB 
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20);

        // save value
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        // convert linear into dB 
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);

        // save value
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}