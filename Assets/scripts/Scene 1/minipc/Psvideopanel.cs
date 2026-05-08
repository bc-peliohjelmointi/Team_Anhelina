using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
// ============================================================
// PSVideoPanel.cs
// ============================================================
// Video panel opened from desktop video icon
// Uses Unity VideoPlayer component
// Has a Back button to return to desktop
//
// HOW TO ADD YOUR VIDEO:
// Option A - VideoClip asset:
//   1. Drag your .mp4 into Assets/Videos/ folder
//   2. Select it -> Inspector -> confirm import
//   3. Drag into the VideoClip field in inspector
//
// Option B - StreamingAssets:
//   1. Create folder: Assets/StreamingAssets/Videos/
//   2. Copy your .mp4 there
//   3. Leave VideoClip empty, set VideoPath = "Videos/yourfile.mp4"
//
// Supported formats: .mp4 (H.264), .webm, .ogv
// ============================================================
public class PSVideoPanel : MonoBehaviour
{
    [Header("Video Player")]
    public VideoPlayer videoPlayer;
    // RawImage that displays the video output
    public RawImage videoDisplay;
    // resolution of the render texture
    public int renderWidth = 1280;
    public int renderHeight = 720;

    [Header("Video Source")]
    // Option A: drag a VideoClip asset here
    public VideoClip videoClip;
    // Option B: path relative to StreamingAssets folder
    // example: "Videos/intro.mp4"
    public string videoPath = "";

    [Header("UI")]
    public Button backButton;
    public Button replayButton;
    // shown while video is loading
    public GameObject loadingIndicator;

    [Header("Audio")]
    public float videoVolume = 0.8f;

    private RenderTexture renderTexture;

    void Start()
    {
        SetupVideoPlayer();
        if (backButton != null) backButton.onClick.AddListener(Close);
        if (replayButton != null) replayButton.onClick.AddListener(Replay);
        if (loadingIndicator != null) loadingIndicator.SetActive(false);
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer == null) return;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.SetDirectAudioVolume(0, videoVolume);

        // assign video source
        if (videoClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
        }
        else if (!string.IsNullOrEmpty(videoPath))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = System.IO.Path.Combine(
                Application.streamingAssetsPath, videoPath);
        }

        // create render texture and assign to RawImage
        renderTexture = new RenderTexture(renderWidth, renderHeight, 0);
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        if (videoDisplay != null) videoDisplay.texture = renderTexture;

        videoPlayer.loopPointReached += OnVideoEnd;
    }

    // called from PSDesktop when video icon is clicked
    public void Open()
    {
        gameObject.SetActive(true);
        Play();
    }

    public void Play()
    {
        if (videoPlayer == null) return;
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    public void Replay()
    {
        videoPlayer.time = 0;
        videoPlayer.Play();
    }

    public void Close()
    {
        if (videoPlayer != null) videoPlayer.Stop();
        videoPlayer.time = 0;
        gameObject.SetActive(false);
        if (PSDesktop.Instance != null) PSDesktop.Instance.ReturnToDesktop();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // video finished - show replay button if assigned
        if (replayButton != null) replayButton.gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        if (videoPlayer != null) videoPlayer.loopPointReached -= OnVideoEnd;
        if (renderTexture != null) renderTexture.Release();
    }
}