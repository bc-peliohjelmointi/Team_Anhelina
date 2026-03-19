using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueSubtitle
{
    [TextArea] public string text;
    public float startTime;
    public float endTime;
}

public class NPCInteraction : MonoBehaviour
{
    [Header("Mission Settings")]
    public int missionID;
    public MissionSystem missionSystem;

    [Header("Dialogue Settings")]
    public AudioClip dialogueAudio;
    public DialogueSubtitle[] subtitles;
    public TextMeshProUGUI subtitleText;
    public float fadeSpeed = 3f;

    [Header("Trigger Mode")]
    public bool autoPlayOnEnter = false; // включи если нужен авто-монолог без E

    private AudioSource audioSource;
    private bool playerNear = false;
    private bool isTalking = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isTalking && !autoPlayOnEnter)
        {
            if (missionSystem == null || missionSystem.GetCurrentMission() == missionID)
            {
                StartCoroutine(PlayDialogue());
                if (missionSystem != null)
                    missionSystem.CompleteMission(missionID);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;

        if (autoPlayOnEnter && !isTalking)
            StartCoroutine(PlayDialogue());
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
    }

    IEnumerator PlayDialogue()
    {
        isTalking = true;

        if (dialogueAudio != null)
        {
            audioSource.clip = dialogueAudio;
            audioSource.Play();
        }

        int currentSubtitle = -1;

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

            if (activeLine != currentSubtitle)
            {
                currentSubtitle = activeLine;
                StopCoroutine("FadeText"); // останавливаем предыдущий fade
                if (currentSubtitle >= 0)
                    StartCoroutine(FadeText(subtitles[currentSubtitle].text, true));
                else
                    StartCoroutine(FadeText("", false));
            }

            yield return null;
        }

        StartCoroutine(FadeText("", false));
        isTalking = false;
    }

    IEnumerator FadeText(string text, bool show)
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