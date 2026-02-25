using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class TVVideoPlayer : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public VideoPlayer videoPlayer;

    [Header("Countdown & Error Materials")]
    public Material countdownMaterial;
    public Material errorMaterial;

    [Header("Episode Videos")]
    public VideoClip episode1Clip;
    public VideoClip episode2Clip;
    public VideoClip episode3Clip;

    [Header("Audio")]
    public AudioSource completionSound;

    private bool isPlaying = false;
    private Renderer tvRenderer;
    private Material originalMaterial;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        tvRenderer = GetComponent<Renderer>();
        if (tvRenderer != null && tvEffect != null)
        {
            originalMaterial = tvEffect.glitchMaterial;
        }
    }

    public void PlayEpisode(int episodeNumber, bool isCorrect)
    {
        if (isPlaying || tvEffect == null || !tvEffect.IsOn())
            return;

        StopAllCoroutines();
        StartCoroutine(PlayEpisodeSequence(episodeNumber, isCorrect));
    }

    IEnumerator PlayEpisodeSequence(int episodeNumber, bool isCorrect)
    {
        isPlaying = true;

        if (isCorrect)
        {
            if (countdownMaterial != null && tvRenderer != null)
            {
                tvRenderer.material = countdownMaterial;
            }

            yield return new WaitForSeconds(3f);

            if (!tvEffect.IsOn())
            {
                ReturnToOriginalState();
                yield break;
            }

            VideoClip clipToPlay = null;
            if (episodeNumber == 1) clipToPlay = episode1Clip;
            else if (episodeNumber == 2) clipToPlay = episode2Clip;
            else if (episodeNumber == 3) clipToPlay = episode3Clip;

            if (clipToPlay != null && videoPlayer != null)
            {
                videoPlayer.clip = clipToPlay;

                RenderTexture renderTexture = new RenderTexture(1920, 1080, 0);
                videoPlayer.targetTexture = renderTexture;

                Material videoMaterial = new Material(Shader.Find("Unlit/Texture"));
                videoMaterial.mainTexture = renderTexture;
                tvRenderer.material = videoMaterial;

                videoPlayer.Play();
            }
        }
        else
        {
            if (errorMaterial != null && tvRenderer != null)
            {
                tvRenderer.material = errorMaterial;
            }

            yield return new WaitForSeconds(3f);

            ReturnToOriginalState();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (completionSound != null)
        {
            completionSound.Play();
        }

        StartCoroutine(ReturnToNoiseAfterDelay());
    }

    IEnumerator ReturnToNoiseAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        ReturnToOriginalState();
    }

    void ReturnToOriginalState()
    {
        if (tvEffect != null && tvEffect.IsOn())
        {
            tvEffect.ReturnToNoise();
        }
        else if (tvEffect != null && !tvEffect.IsOn())
        {
            if (tvRenderer != null && tvEffect.offScreenMaterial != null)
            {
                tvRenderer.material = tvEffect.offScreenMaterial;
            }
        }

        isPlaying = false;
    }
}