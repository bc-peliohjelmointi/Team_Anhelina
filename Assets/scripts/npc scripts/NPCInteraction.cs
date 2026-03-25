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
    public bool autoPlayOnEnter = false;

    private AudioSource audioSource;
    private bool playerNear = false;
    private bool isTalking = false;
    private CharacterController playerController;

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
        playerController = other.GetComponent<CharacterController>();

        if (!isTalking && !autoPlayOnEnter)
            InteractionHint.instance.Show("Press E to talk");

        if (autoPlayOnEnter && !isTalking)
            StartCoroutine(PlayDialogue());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        playerController = null;
        InteractionHint.instance.Hide();
        StopDialogue();
    }

    void SetPlayerMovement(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }

    void StopDialogue()
    {
        if (!isTalking) return;

        StopAllCoroutines();
        audioSource.Stop();
        isTalking = false;

        SetPlayerMovement(true);

        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    IEnumerator PlayDialogue()
    {
        isTalking = true;
        InteractionHint.instance.Hide();
        SetPlayerMovement(false);

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
                StopCoroutine("FadeText");

                if (currentSubtitle >= 0)
                    StartCoroutine(FadeText(subtitles[currentSubtitle].text, true));
                else
                    StartCoroutine(FadeText("", false));
            }

            yield return null;
        }

        StartCoroutine(FadeText("", false));
        SetPlayerMovement(true);
        isTalking = false;

        if (playerNear)
            InteractionHint.instance.Show("Press E to talk");
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