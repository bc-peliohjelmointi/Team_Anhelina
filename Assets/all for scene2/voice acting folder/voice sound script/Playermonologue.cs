using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class SubtitleLine
{
    [TextArea] public string text;   // текст субтитров
    public float startTime;           // время появления (сек)
    public float endTime;             // время скрытия (сек)
}

public class Playermonologue : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip monologueClip;

    [Header("Subtitles")]
    public SubtitleLine[] subtitles;  // массив субтитров с таймкодами
    public TextMeshProUGUI subtitleText;
    public float fadeSpeed = 3f;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;
    public MonoBehaviour playerLookScript;

    private AudioSource audioSource;
    private bool isPlaying = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D звук

        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    public void StartMonologue()
    {
        if (isPlaying) return;
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        isPlaying = true;

        // Блокируем управление игроком
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (playerLookScript != null) playerLookScript.enabled = false;

        if (monologueClip != null)
        {
            audioSource.clip = monologueClip;
            audioSource.Play();
        }

        int currentLine = -1;

        while (audioSource.isPlaying)
        {
            float elapsed = audioSource.time;

            int activeLine = -1;
            for (int i = 0; i < subtitles.Length; i++)
            {
                if (elapsed >= subtitles[i].startTime && elapsed < subtitles[i].endTime)
                {
                    activeLine = i;
                    break;
                }
            }

            if (activeLine != currentLine)
            {
                currentLine = activeLine;
                if (currentLine >= 0)
                    StartCoroutine(FadeText(subtitles[currentLine].text, true));
                else
                    StartCoroutine(FadeText("", false));
            }

            yield return null;
        }

        // После окончания аудио
        StartCoroutine(FadeText("", false));

        // Разблокируем игрока
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (playerLookScript != null) playerLookScript.enabled = true;

        isPlaying = false;
    }

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
}