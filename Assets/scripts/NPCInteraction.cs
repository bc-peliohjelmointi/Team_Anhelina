using System.Collections;
using UnityEngine;
using TMPro;

[System.Serializable]
public class DialogueSubtitle
{
    [TextArea] public string text;   // текст субтитров
    public float startTime;           // когда показывать
    public float endTime;             // когда скрывать
}

public class NPCInteraction : MonoBehaviour
{
    [Header("Mission Settings")]
    public int missionID;
    public MissionSystem missionSystem;

    [Header("Dialogue Settings")]
    public AudioClip dialogueAudio;           // один аудиофайл
    public DialogueSubtitle[] subtitles;      // массив субтитров
    public TextMeshProUGUI subtitleText;     // TMP объект
    public float fadeSpeed = 3f;

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
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isTalking)
        {
            if (missionSystem == null || missionSystem.GetCurrentMission() == missionID)
            {
                StartCoroutine(PlayDialogue());

                if (missionSystem != null)
                    missionSystem.CompleteMission(missionID);
            }
            if (playerNear && Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E нажата, playerNear = true");
            }
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
    }

}