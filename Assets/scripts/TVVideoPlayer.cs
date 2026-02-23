using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class TVVideoPlayer : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public VideoPlayer videoPlayer;

    public Material countdownMaterial;
    public Material errorMaterial;

    public VideoClip episode1Clip;
    public VideoClip episode2Clip;
    public VideoClip episode3Clip;

    public AudioSource completionSound;

    private bool isPlaying = false;

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
            if (countdownMaterial != null && tvEffect != null)
            {
                tvEffect.SetMaterial(countdownMaterial);
            }

            yield return new WaitForSeconds(3f);

            VideoClip clipToPlay = null;
            if (episodeNumber == 1) clipToPlay = episode1Clip;
            else if (episodeNumber == 2) clipToPlay = episode2Clip;
            else if (episodeNumber == 3) clipToPlay = episode3Clip;

            if (clipToPlay != null && videoPlayer != null)
            {
                videoPlayer.clip = clipToPlay;
                videoPlayer.Play();
            }
        }
        else
        {
            if (errorMaterial != null && tvEffect != null)
            {
                tvEffect.SetMaterial(errorMaterial);
            }

            yield return new WaitForSeconds(3f);

            if (tvEffect != null)
            {
                tvEffect.ReturnToNoise();
            }

            isPlaying = false;
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

        if (tvEffect != null)
        {
            tvEffect.ReturnToNoise();
        }

        isPlaying = false;
    }
}