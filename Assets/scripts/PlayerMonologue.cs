using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class MonologueLine
{
    [TextArea] public string text;
    public float startTime;
    public float endTime;
}

public class PlayerMonologue : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip clip;

    [Header("Subtitles")]
    public MonologueLine[] lines;
    public TextMeshProUGUI subtitleText;
    public float fadeSpeed = 3f;

    private AudioSource audioSource;
    private bool played = false;
    private CharacterController playerController;

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

    void OnTriggerEnter(Collider other)
    {
        if (played) return;
        if (!other.CompareTag("Player")) return;

        played = true;
        playerController = other.GetComponent<CharacterController>();
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        if (playerController != null)
            playerController.enabled = false;

        audioSource.clip = clip;
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
                StopCoroutine("Fade");
                StartCoroutine(Fade(current >= 0 ? lines[current].text : "", current >= 0));
            }

            yield return null;
        }

        StartCoroutine(Fade("", false));

        if (playerController != null)
            playerController.enabled = true;
    }

    IEnumerator Fade(string text, bool show)
    {
        if (subtitleText == null) yield break;

        Color c = subtitleText.color;

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed * 2f;
            c.a = Mathf.Max(c.a, 0f);
            subtitleText.color = c;
            yield return null;
        }

        subtitleText.text = text;
        if (!show) yield break;

        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Min(c.a, 1f);
            subtitleText.color = c;
            yield return null;
        }
    }
}