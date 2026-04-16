using System.Collections;
using UnityEngine;
using TMPro;

public class BoundaryTrigger : MonoBehaviour
{
    [Header("К какой миссии относится (0 = бабушка, 1 = гопники)")]
    public int missionType = 0;

    [Header("Ссылка на MissionSystem")]
    public MissionSystem missionSystem;

    [Header("Аудио")]
    public AudioClip clip;

    [Header("Субтитры")]
    public MonologueLine[] lines;
    public TextMeshProUGUI subtitleText;
    public float fadeSpeed = 3f;

    [Header("Настройки")]
    public float cooldown = 10f; // секунды до повторного срабатывания

    AudioSource audioSource;
    Collider col;
    float lastTriggeredAt = -99f;
    bool isPlaying = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        col = GetComponent<Collider>();

        if (subtitleText != null)
        {
            subtitleText.text = "";
            var c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    void Start()
    {
        if (missionSystem != null)
            missionSystem.OnMissionChanged += _ => UpdateActive();

        UpdateActive();
    }

    void OnDestroy()
    {
        if (missionSystem != null)
            missionSystem.OnMissionChanged -= _ => UpdateActive();
    }

    void UpdateActive()
    {
        if (missionSystem == null) return;
        col.enabled = missionSystem.GetCurrentMission() == missionType;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlaying) return;
        if (Time.time - lastTriggeredAt < cooldown) return;

        lastTriggeredAt = Time.time;
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        isPlaying = true;

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
        isPlaying = false;
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