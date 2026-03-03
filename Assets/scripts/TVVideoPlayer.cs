using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class TVVideoPlayer : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public Renderer tvRenderer;

    [Header("Video Quads - Intro")]
    public GameObject introQuad;
    public float introDisplayDuration = 3f;

    [Header("Video Quads - Correct Order")]
    public GameObject[] episode1CorrectQuads;
    public GameObject[] episode2CorrectQuads;
    public GameObject[] episode3CorrectQuads;

    [Header("Video Quads - Error")]
    public GameObject errorQuad;

    [Header("Quad Settings")]
    public float quadDisplayDuration = 3f;
    public bool returnToNoiseAfterQuad = true;

    [Header("Audio")]
    public AudioSource completionSound;

    private bool isPlaying = false;

    void Start()
    {
        if (tvRenderer == null)
        {
            tvRenderer = GetComponent<Renderer>();
        }

        HideAllQuads();
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

        if (tvRenderer != null)
        {
            tvRenderer.enabled = false;
        }

        if (introQuad != null)
        {
            introQuad.SetActive(true);
            yield return new WaitForSeconds(introDisplayDuration);
            introQuad.SetActive(false);
        }

        if (isCorrect)
        {
            GameObject[] quadsToPlay = GetQuadsForEpisode(episodeNumber);

            for (int i = 0; i < quadsToPlay.Length; i++)
            {
                if (!tvEffect.IsOn())
                {
                    ReturnToOriginalState();
                    yield break;
                }

                if (quadsToPlay[i] != null)
                {
                    quadsToPlay[i].SetActive(true);
                    yield return new WaitForSeconds(quadDisplayDuration);
                    quadsToPlay[i].SetActive(false);
                }
            }

            if (completionSound != null)
            {
                completionSound.Play();
            }

            yield return new WaitForSeconds(1f);

            if (returnToNoiseAfterQuad)
            {
                ReturnToOriginalState();
            }
            else
            {
                if (tvRenderer != null)
                {
                    tvRenderer.enabled = true;
                }
                isPlaying = false;
            }
        }
        else
        {
            if (errorQuad != null)
            {
                errorQuad.SetActive(true);
                yield return new WaitForSeconds(quadDisplayDuration);
                errorQuad.SetActive(false);
            }

            ReturnToOriginalState();
        }
    }

    GameObject[] GetQuadsForEpisode(int episodeNumber)
    {
        if (episodeNumber == 1) return episode1CorrectQuads;
        if (episodeNumber == 2) return episode2CorrectQuads;
        if (episodeNumber == 3) return episode3CorrectQuads;
        return new GameObject[0];
    }

    void ReturnToOriginalState()
    {
        HideAllQuads();

        if (tvRenderer != null)
        {
            tvRenderer.enabled = true;
        }

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

    void HideAllQuads()
    {
        if (introQuad != null)
        {
            introQuad.SetActive(false);
        }

        if (episode1CorrectQuads != null)
        {
            foreach (GameObject quad in episode1CorrectQuads)
            {
                if (quad != null) quad.SetActive(false);
            }
        }

        if (episode2CorrectQuads != null)
        {
            foreach (GameObject quad in episode2CorrectQuads)
            {
                if (quad != null) quad.SetActive(false);
            }
        }

        if (episode3CorrectQuads != null)
        {
            foreach (GameObject quad in episode3CorrectQuads)
            {
                if (quad != null) quad.SetActive(false);
            }
        }

        if (errorQuad != null)
        {
            errorQuad.SetActive(false);
        }
    }

    void OnDestroy()
    {
        HideAllQuads();
    }
}