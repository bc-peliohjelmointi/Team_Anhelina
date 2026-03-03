using System.Collections;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Video;

public class TVVideoPlayer : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public Renderer tvRenderer;

    [Header("Video Quads - Intro")]
    public GameObject introQuad;
    public float introDisplayDuration = 3f;

    [Header("Video Quads - Correct Order (Full Episodes)")]
    public GameObject[] episode1CorrectQuads;
    public GameObject[] episode2CorrectQuads;
    public GameObject[] episode3CorrectQuads;

    [Header("Video Quads - Wrong Order (Short Cartoons)")]
    public GameObject episode1WrongCartoonQuad;
    public GameObject episode2WrongCartoonQuad;
    public GameObject episode3WrongCartoonQuad;
    public float wrongCartoonDuration = 5f;

    [Header("Error Quad")]
    public GameObject errorQuad;
    public float errorDisplayDuration = 3f;

    [Header("Quad Settings")]
    public float quadDisplayDuration = 3f;

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

        if (!tvEffect.IsOn())
        {
            ReturnToOriginalState();
            yield break;
        }

        if (isCorrect)
        {
            GameObject[] quadsToPlay = GetCorrectQuadsForEpisode(episodeNumber);

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

            ReturnToOriginalState();
        }
        else
        {
            GameObject wrongCartoon = GetWrongCartoonForEpisode(episodeNumber);

            if (wrongCartoon != null)
            {
                wrongCartoon.SetActive(true);
                yield return new WaitForSeconds(wrongCartoonDuration);
                wrongCartoon.SetActive(false);
            }

            if (!tvEffect.IsOn())
            {
                ReturnToOriginalState();
                yield break;
            }

            if (errorQuad != null)
            {
                errorQuad.SetActive(true);
                yield return new WaitForSeconds(errorDisplayDuration);
                errorQuad.SetActive(false);
            }

            ReturnToOriginalState();
        }
    }

    GameObject[] GetCorrectQuadsForEpisode(int episodeNumber)
    {
        if (episodeNumber == 1) return episode1CorrectQuads;
        if (episodeNumber == 2) return episode2CorrectQuads;
        if (episodeNumber == 3) return episode3CorrectQuads;
        return new GameObject[0];
    }

    GameObject GetWrongCartoonForEpisode(int episodeNumber)
    {
        if (episodeNumber == 1) return episode1WrongCartoonQuad;
        if (episodeNumber == 2) return episode2WrongCartoonQuad;
        if (episodeNumber == 3) return episode3WrongCartoonQuad;
        return null;
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

        if (episode1WrongCartoonQuad != null)
        {
            episode1WrongCartoonQuad.SetActive(false);
        }

        if (episode2WrongCartoonQuad != null)
        {
            episode2WrongCartoonQuad.SetActive(false);
        }

        if (episode3WrongCartoonQuad != null)
        {
            episode3WrongCartoonQuad.SetActive(false);
        }

        if (errorQuad != null)
        {
            errorQuad.SetActive(false);
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
    }

    void OnDestroy()
    {
        HideAllQuads();
    }
}
