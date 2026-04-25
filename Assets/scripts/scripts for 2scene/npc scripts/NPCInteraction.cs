using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
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

    [Header("NPC Movement")]
    public NavMeshAgent navAgent;
    public Transform seatTransform;
    public float returnStopDistance = 0.3f;

    [Header("Player Stand Point")]
    public Transform playerStandPoint;

    [Header("Screen Fade")]
    public Image fadePanel;
    public float fadeDuration = 0.4f;

    [Header("Skip Hint")]
    public TextMeshProUGUI skipHintText;

    private AudioSource audioSource;
    private bool playerNear = false;
    private bool isTalking = false;
    private bool isReturning = false;
    private CharacterController playerController;
    private Quaternion initialRotation;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, 0f);
        }

        if (fadePanel != null)
            fadePanel.color = new Color(0f, 0f, 0f, 0f);

        if (skipHintText != null)
            skipHintText.gameObject.SetActive(false);

        initialRotation = transform.rotation;

        if (navAgent != null)
            navAgent.enabled = false;
    }

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isTalking && !autoPlayOnEnter && !isReturning)
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

        if (!isTalking && !autoPlayOnEnter && !isReturning)
            InteractionHint.instance.Show("Press E to talk");

        if (autoPlayOnEnter && !isTalking && !isReturning)
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

        if (fadePanel != null)
            fadePanel.color = new Color(0f, 0f, 0f, 0f);

        if (skipHintText != null)
            skipHintText.gameObject.SetActive(false);

        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.color = new Color(subtitleText.color.r, subtitleText.color.g, subtitleText.color.b, 0f);
        }

        StartCoroutine(ReturnToSeat());
    }

    IEnumerator PlayDialogue()
    {
        isTalking = true;
        InteractionHint.instance.Hide();
        SetPlayerMovement(false);

        yield return StartCoroutine(ScreenFade(0f, 1f));

        if (playerStandPoint != null && playerController != null)
        {
            playerController.enabled = false;
            playerController.transform.position = playerStandPoint.position;

            Vector3 dir = transform.position - playerController.transform.position;
            dir.y = 0f;

            if (dir != Vector3.zero)
                playerController.transform.rotation = Quaternion.LookRotation(dir);
        }

        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(ScreenFade(1f, 0f));

        if (skipHintText != null)
            skipHintText.gameObject.SetActive(true);

        if (dialogueAudio != null)
        {
            audioSource.clip = dialogueAudio;
            audioSource.Play();
        }

        int currentSubtitle = -1;

        while (audioSource.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                audioSource.Stop();
                break;
            }

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

                StartCoroutine(activeLine >= 0
                    ? FadeText(subtitles[activeLine].text, true)
                    : FadeText("", false));
            }

            yield return null;
        }

        if (skipHintText != null)
            skipHintText.gameObject.SetActive(false);

        StartCoroutine(FadeText("", false));

        SetPlayerMovement(true);
        isTalking = false;

        StartCoroutine(ReturnToSeat());

        if (playerNear)
            InteractionHint.instance.Show("Press E to talk");
    }

    IEnumerator ScreenFade(float from, float to)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            fadePanel.color = new Color
            (
                0f,
                0f,
                0f,
                Mathf.Lerp(from, to, elapsed / fadeDuration)
            );

            yield return null;
        }

        fadePanel.color = new Color(0f, 0f, 0f, to);
    }

    IEnumerator ReturnToSeat()
    {
        if (seatTransform == null || navAgent == null) yield break;
        if (isReturning) yield break;

        isReturning = true;

        navAgent.enabled = true;
        navAgent.isStopped = false;
        navAgent.SetDestination(seatTransform.position);

        while (true)
        {
            float dist = Vector3.Distance(transform.position, seatTransform.position);

            if (dist <= returnStopDistance) break;

            if (!navAgent.pathPending && navAgent.remainingDistance <= returnStopDistance)
                break;

            yield return null;
        }

        navAgent.isStopped = true;
        navAgent.enabled = false;

        yield return StartCoroutine(RotateToInitial());

        isReturning = false;
    }

    IEnumerator RotateToInitial()
    {
        float duration = 0.5f;
        float elapsed = 0f;

        Quaternion from = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            transform.rotation = Quaternion.Slerp
            (
                from,
                initialRotation,
                elapsed / duration
            );

            yield return null;
        }

        transform.rotation = initialRotation;
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