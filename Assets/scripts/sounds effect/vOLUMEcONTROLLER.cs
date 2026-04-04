using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VolumeController : MonoBehaviour
{
    public Slider masterSlider; // master volume UI
    public Slider musicSlider;  // music volume UI
    public Slider uiSlider;     // UI volume UI

    public AudioSource musicSource; // music audio source
    public AudioSource uiSource;    // UI audio source

    public VideoPlayer videoPlayer; // video player 

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float uiVolume = 1f;

    void Start()
    {
        // load saved values
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 1f);

        // set sliders
        masterSlider.value = masterVolume;
        musicSlider.value = musicVolume;
        uiSlider.value = uiVolume;

        // listen to slider changes
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);

        ApplyVolumes(); // apply at start
    }

    void SetMasterVolume(float value)
    {
        masterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", value); // save
        ApplyVolumes(); // update audio
    }

    void SetMusicVolume(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value); // save
        ApplyVolumes();
    }

    void SetUIVolume(float value)
    {
        uiVolume = value;
        PlayerPrefs.SetFloat("UIVolume", value); // save
        ApplyVolumes();
    }

    void ApplyVolumes()
    {
        // apply master volume to all audio
        AudioListener.volume = masterVolume;

        // apply music volume
        musicSource.volume = musicVolume;

        // apply UI volume
        uiSource.volume = uiVolume;

        if (videoPlayer != null)
        {
            // check if video has audio
            if (videoPlayer.audioTrackCount > 0)
            {
                videoPlayer.SetDirectAudioVolume(0, masterVolume); // apply to video
            }
        }
    }
}