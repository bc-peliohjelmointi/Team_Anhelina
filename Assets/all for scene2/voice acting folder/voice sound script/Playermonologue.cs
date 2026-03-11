using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Playermonologue : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip monologueClip;

    [Header("Subtitles UI")]
    public TextMeshProUGUI subtitleText; // перетащи TMP объект сюда
    // CanvasGroup НЕ нужен — fade работает через цвет текста

    [Header("Subtitle Lines (синхронизируй с аудио)")]
    public SubtitleLine[] subtitleLines = new SubtitleLine[]
    {
        new SubtitleLine(0.0f,  3.5f,  "It's already so dark,"),
        new SubtitleLine(3.5f,  7.5f,  "and I still have to stop by\nGrandma Tamara's place for a visit."),
        new SubtitleLine(7.5f,  12.0f, "It's strange how the streetlights\nare shining along the road I need to take."),
        new SubtitleLine(12.0f, 17.0f, "Something tells me it might be better\nnot to walk in the dark…"),
    };

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour playerLookScript;

    [Header("Settings")]
    public bool playOnStart = true;
    public float fadeSpeed = 3f;

    private AudioSource audioSource;
    private bool isPlaying = false;
    private bool triggered = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        // Скрыть текст через прозрачность
        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    void Start()
    {
        if (playOnStart)
            StartMonologue();
    }

    public void StartMonologue()
    {
        if (triggered || isPlaying) return;
        triggered = true;
        StartCoroutine(PlayMonologue());
    }

    IEnumerator PlayMonologue()
    {
        isPlaying = true;
        SetPlayerControl(false);

        yield return new WaitForSeconds(0.5f);

        if (monologueClip != null)
        {
            audioSource.clip = monologueClip;
            audioSource.Play();
        }

        float startTime = Time.time;
        int currentLine = -1;

        float audioDuration = monologueClip != null
            ? monologueClip.length
            : GetLastSubtitleTime();

        while (Time.time - startTime < audioDuration)
        {
            float elapsed = Time.time - startTime;

            int activeLine = -1;
            for (int i = 0; i < subtitleLines.Length; i++)
            {
                if (elapsed >= subtitleLines[i].startTime &&
                    elapsed < subtitleLines[i].endTime)
                {
                    activeLine = i;
                    break;
                }
            }

            if (activeLine != currentLine)
            {
                currentLine = activeLine;

                if (activeLine >= 0)
                    yield return StartCoroutine(FadeText(subtitleLines[activeLine].text, true));
                else
                    yield return StartCoroutine(FadeText("", false));
            }

            yield return null;
        }

        yield return StartCoroutine(FadeText("", false));

        SetPlayerControl(true);
        isPlaying = false;
    }

    // Fade через alpha цвета TMP — без CanvasGroup
    IEnumerator FadeText(string text, bool show)
    {
        if (subtitleText == null) yield break;

        Color c = subtitleText.color;

        // Fade OUT
        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed * 2f;
            c.a = Mathf.Max(c.a, 0f);
            subtitleText.color = c;
            yield return null;
        }

        subtitleText.text = text;
        if (!show) yield break;

        // Fade IN
        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Min(c.a, 1f);
            subtitleText.color = c;
            yield return null;
        }
    }

    void SetPlayerControl(bool enabled)
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = enabled;

        if (playerLookScript != null)
            playerLookScript.enabled = enabled;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = !enabled;
            if (!enabled) rb.linearVelocity = Vector3.zero;
        }
    }

    float GetLastSubtitleTime()
    {
        float max = 5f;
        foreach (var line in subtitleLines)
            if (line.endTime > max) max = line.endTime;
        return max + 0.5f;
    }
}

[System.Serializable]
public class SubtitleLine
{
    public float startTime;
    public float endTime;
    [TextArea] public string text;

    public SubtitleLine(float start, float end, string t)
    {
        startTime = start;
        endTime = end;
        text = t;
    }
}