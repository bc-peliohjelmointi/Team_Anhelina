using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class TVVideoPlayer : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public VideoPlayer videoPlayer;
    public Renderer tvRenderer;

    [Header("Countdown & Error")]
    public VideoClip countdownVideo;
    public VideoClip errorVideo;

    [Header("Episode Videos")]
    public VideoClip episode1Clip;
    public VideoClip episode2Clip;
    public VideoClip episode3Clip;

    [Header("Audio")]
    public AudioSource completionSound;

    private bool isPlaying = false;
    private RenderTexture renderTexture;
    private Material videoMaterial;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        if (tvRenderer == null)
        {
            tvRenderer = GetComponent<Renderer>();
        }

        renderTexture = new RenderTexture(1920, 1080, 0);

        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoEnd;

        videoMaterial = new Material(Shader.Find("Unlit/Texture"));
        videoMaterial.mainTexture = renderTexture;
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
            if (countdownVideo != null)
            {
                videoPlayer.clip = countdownVideo;
                tvRenderer.material = videoMaterial;
                videoPlayer.Play();

                while (videoPlayer.isPlaying)
                {
                    if (!tvEffect.IsOn())
                    {
                        videoPlayer.Stop();
                        ReturnToOriginalState();
                        yield break;
                    }
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

            if (!tvEffect.IsOn())
            {
                ReturnToOriginalState();
                yield break;
            }

            VideoClip clipToPlay = null;
            if (episodeNumber == 1) clipToPlay = episode1Clip;
            else if (episodeNumber == 2) clipToPlay = episode2Clip;
            else if (episodeNumber == 3) clipToPlay = episode3Clip;

            if (clipToPlay != null)
            {
                videoPlayer.clip = clipToPlay;
                tvRenderer.material = videoMaterial;
                videoPlayer.Play();
            }
        }
        else
        {
            if (errorVideo != null)
            {
                videoPlayer.clip = errorVideo;
                tvRenderer.material = videoMaterial;
                videoPlayer.Play();

                while (videoPlayer.isPlaying)
                {
                    if (!tvEffect.IsOn())
                    {
                        videoPlayer.Stop();
                        ReturnToOriginalState();
                        yield break;
                    }
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

            ReturnToOriginalState();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (vp.clip == episode1Clip || vp.clip == episode2Clip || vp.clip == episode3Clip)
        {
            if (completionSound != null)
            {
                completionSound.Play();
            }

            StartCoroutine(ReturnToNoiseAfterDelay());
        }
    }

    IEnumerator ReturnToNoiseAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        ReturnToOriginalState();
    }

    void ReturnToOriginalState()
    {
        videoPlayer.Stop();

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

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }
}