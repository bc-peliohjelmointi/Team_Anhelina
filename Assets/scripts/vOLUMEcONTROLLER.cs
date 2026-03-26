using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VolumeController : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider uiSlider;

    public AudioSource musicSource; 
    public AudioSource uiSource;

    public VideoPlayer videoPlayer;

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float uiVolume = 1f;

    void Start()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 1f);

        masterSlider.value = masterVolume;
        musicSlider.value = musicVolume;
        uiSlider.value = uiVolume;

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);

        ApplyVolumes();
    }

    void SetMasterVolume(float value)
    {
        masterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        ApplyVolumes();
    }

    void SetMusicVolume(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);
        ApplyVolumes();
    }

    void SetUIVolume(float value)
    {
        uiVolume = value;
        PlayerPrefs.SetFloat("UIVolume", value);
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        // master all sounds
        AudioListener.volume = masterVolume;

        // Music only
        musicSource.volume = musicVolume;

        // UI only
        uiSource.volume = uiVolume;

        if (videoPlayer != null)
        {
            // đảm bảo có audio track
            if (videoPlayer.audioTrackCount > 0)
            {
                videoPlayer.SetDirectAudioVolume(0, masterVolume);
            }
        }
    }


}