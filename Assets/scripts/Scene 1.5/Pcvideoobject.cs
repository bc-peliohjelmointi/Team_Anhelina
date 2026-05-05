using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
// special object on the PC desktop that plays a video
// player clicks it → all PC UI hides → video plays fullscreen on PC canvas
// when video ends → all PC UI returns → object can be clicked again
// uses Unity VideoPlayer component — assign a VideoClip in inspector
// or set a URL path to a video file in StreamingAssets
// put this script on the clickable video object button
public class PCVideoObject : MonoBehaviour
{
    // ---- references ----
    // the VideoPlayer component that plays the clip
    public VideoPlayer videoPlayer;
    // RawImage that shows the video output texture
    public RawImage videoDisplay;
    // the panel containing the video display, hidden until played
    public GameObject videoPanel;
    // ALL other panels on the PC canvas that should hide during video
    // drag in: FolderPanel, CodePanel, MiniGamePanel, DesktopPanel etc.
    public GameObject[] panelsToHide;
    // button or object the player clicks to start video
    // put an AuraHighlight here too
    public AuraHighlight auraHighlight;
    // "E - Watch" prompt
    public GameObject interactionPrompt;
    // back button shown during/after video to return to desktop
    public Button backButton;

    // ---- video settings ----
    // drag your VideoClip asset here
    public VideoClip videoClip;
    // OR leave videoClip null and set a path in StreamingAssets folder
    // example: "Videos/intro.mp4"
    public string videoFilePath = "";
    // how fast audio fades in at video start
    public float audioFadeInSpeed = 2f;

    // ---- interaction ----
    public float interactionDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;

    // ---- state ----
    private bool isPlayingVideo = false;
    private bool isNearby = false;
    // remembers which panels were active before hiding them
    private bool[] panelWasActive;

    void Start()
    {
        if (videoPanel != null) videoPanel.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
            backButton.onClick.AddListener(StopVideo);
        }
        SetupVideoPlayer();
        panelWasActive = new bool[panelsToHide != null ? panelsToHide.Length : 0];
    }

    void SetupVideoPlayer()
    {
        if (videoPlayer == null) return;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        // assign clip or URL
        if (videoClip != null)
            videoPlayer.clip = videoClip;
        else if (!string.IsNullOrEmpty(videoFilePath))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, videoFilePath);
        }
        // render to RawImage texture
        if (videoDisplay != null)
        {
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            RenderTexture rt = new RenderTexture(1280, 720, 0);
            videoPlayer.targetTexture = rt;
            videoDisplay.texture = rt;
        }
        // subscribe to end event
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        if (isPlayingVideo) return;
        Camera cam = Camera.main;
        if (cam == null) return;
        bool lookingAt = Physics.Raycast(
            new Ray(cam.transform.position, cam.transform.forward),
            out RaycastHit hit, interactionDistance)
            && hit.collider.gameObject == gameObject;
        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (auraHighlight != null) auraHighlight.SetGlow(isNearby);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }
        if (isNearby && Input.GetKeyDown(interactKey)) PlayVideo();
    }

    public void PlayVideo()
    {
        if (isPlayingVideo || videoPlayer == null) return;
        isPlayingVideo = true;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (auraHighlight != null) auraHighlight.SetGlow(false);
        // remember and hide all other panels
        if (panelsToHide != null)
        {
            for (int i = 0; i < panelsToHide.Length; i++)
            {
                if (panelsToHide[i] == null) continue;
                panelWasActive[i] = panelsToHide[i].activeSelf;
                panelsToHide[i].SetActive(false);
            }
        }
        // show video panel
        if (videoPanel != null) videoPanel.SetActive(true);
        if (backButton != null) backButton.gameObject.SetActive(true);
        // play
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // video finished naturally
        StartCoroutine(ReturnToPreviousState());
    }

    public void StopVideo()
    {
        if (!isPlayingVideo) return;
        if (videoPlayer != null) videoPlayer.Stop();
        StartCoroutine(ReturnToPreviousState());
    }

    IEnumerator ReturnToPreviousState()
    {
        yield return null; // wait one frame
        isPlayingVideo = false;
        if (videoPanel != null) videoPanel.SetActive(false);
        if (backButton != null) backButton.gameObject.SetActive(false);
        // restore panels that were visible before
        if (panelsToHide != null)
        {
            for (int i = 0; i < panelsToHide.Length; i++)
            {
                if (panelsToHide[i] == null) continue;
                panelsToHide[i].SetActive(panelWasActive[i]);
            }
        }
        // reset video to beginning so it can be watched again
        if (videoPlayer != null) videoPlayer.time = 0;
    }

    void OnDestroy()
    {
        if (videoPlayer != null) videoPlayer.loopPointReached -= OnVideoEnd;
    }
}