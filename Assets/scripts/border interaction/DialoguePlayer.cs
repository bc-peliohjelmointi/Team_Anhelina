using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueLine
{
    [TextArea] public string text;
    public float startTime;
    public float endTime;
}

public class DialoguePlayer : MonoBehaviour
{
    public AudioClip audioClip;
    public DialogueLine[] lines;
    public TextMeshProUGUI subtitleText;

    public float fadeSpeed = 3f;

    AudioSource audioSource;
    bool isPlaying = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        if (subtitleText != null)
        {
            subtitleText.text = "";
            var c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    public void Play()
    {
        if (isPlaying) return;
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        isPlaying = true;

        audioSource.clip = audioClip;
        audioSource.Play();

        int current = -1;

        while (audioSource.isPlaying)
        {
            float t = audioSource.time;
            int active = -1;

            for (int i = 0; i < lines.Length; i++)
            {
                if (t >= lines[i].startTime && t < lines[i].endTime)
                {
                    active = i;
                    break;
                }
            }

            if (active != current)
            {
                current = active;
                StopCoroutine("FadeText");

                if (current >= 0)
                    StartCoroutine(FadeText(lines[current].text, true));
                else
                    StartCoroutine(FadeText("", false));
            }

            yield return null;
        }

        StartCoroutine(FadeText("", false));
        isPlaying = false;
    }

    IEnumerator FadeText(string text, bool show)
    {
        if (subtitleText == null) yield break;

        Color c = subtitleText.color;

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed * 2f;
            subtitleText.color = c;
            yield return null;
        }

        subtitleText.text = text;

        if (!show) yield break;

        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            subtitleText.color = c;
            yield return null;
        }
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }
}