using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BusEndingTrigger : MonoBehaviour
{
    [Header("Настройки")]
    public float fadeDuration = 2f;
    public float displayTime = 60f;
    public string endingText = "Продолжение следует";
    public int mainMenuSceneIndex = 0;

    [Header("Звук")]
    public AudioClip endingMusic;           // Твой финальный трек
    public float musicFadeInDuration = 2f;  // Нарастание громкости
    public float soundFadeOutDuration = 1f; // Затухание всех звуков

    [Header("UI (можно задать своё или создастся автоматически)")]
    public Canvas endingCanvas;
    public Image fadePanel;
    public TextMeshProUGUI endingLabel;

    private bool _triggered = false;
    private AudioSource _endingAudioSource;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        MissionSystem missionSystem = FindObjectOfType<MissionSystem>();
        if (missionSystem == null || !missionSystem.IsLastMission()) return;

        _triggered = true;
        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        SetupUI();
        SetupAudio();

        // --- 1. Глушим все звуки и запускаем финальную музыку ---
        StartCoroutine(FadeOutAllSounds(soundFadeOutDuration));
        StartCoroutine(FadeInEndingMusic(musicFadeInDuration));

        // --- 2. Fade Out экрана ---
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            fadePanel.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadePanel.color = Color.black;

        // --- 3. Надпись появляется ---
        endingLabel.gameObject.SetActive(true);
        t = 0f;
        float textFadeDuration = 1.5f;
        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / textFadeDuration);
            endingLabel.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        // --- 4. Ждём ---
        yield return new WaitForSeconds(displayTime);

        // --- 5. Загружаем главное меню ---
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void SetupAudio()
    {
        // Создаём отдельный AudioSource для финальной музыки
        _endingAudioSource = gameObject.AddComponent<AudioSource>();
        _endingAudioSource.clip = endingMusic;
        _endingAudioSource.loop = true;
        _endingAudioSource.volume = 0f;
        _endingAudioSource.Play();
    }

    // Плавно глушим все AudioSource на сцене, кроме нашего финального
    private IEnumerator FadeOutAllSounds(float duration)
    {
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();

        // Запоминаем начальные громкости
        float[] startVolumes = new float[allSources.Length];
        for (int i = 0; i < allSources.Length; i++)
            startVolumes[i] = allSources[i].volume;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float factor = 1f - Mathf.Clamp01(t / duration);

            for (int i = 0; i < allSources.Length; i++)
            {
                if (allSources[i] == _endingAudioSource) continue; // наш не трогаем
                if (allSources[i] != null)
                    allSources[i].volume = startVolumes[i] * factor;
            }

            yield return null;
        }

        // Полностью останавливаем все лишние источники
        foreach (var source in allSources)
        {
            if (source == _endingAudioSource) continue;
            if (source != null) source.Stop();
        }
    }

    // Плавно поднимаем громкость финальной музыки
    private IEnumerator FadeInEndingMusic(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _endingAudioSource.volume = Mathf.Clamp01(t / duration);
            yield return null;
        }
        _endingAudioSource.volume = 1f;
    }

    private void SetupUI()
    {
        if (endingCanvas == null)
        {
            GameObject canvasObj = new GameObject("EndingCanvas");
            endingCanvas = canvasObj.AddComponent<Canvas>();
            endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            endingCanvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (fadePanel == null)
        {
            GameObject panelObj = new GameObject("FadePanel");
            panelObj.transform.SetParent(endingCanvas.transform, false);
            fadePanel = panelObj.AddComponent<Image>();
            fadePanel.color = new Color(0f, 0f, 0f, 0f);
            RectTransform rt = fadePanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        fadePanel.gameObject.SetActive(true);

        if (endingLabel == null)
        {
            GameObject textObj = new GameObject("EndingText");
            textObj.transform.SetParent(endingCanvas.transform, false);
            endingLabel = textObj.AddComponent<TextMeshProUGUI>();
            endingLabel.text = endingText;
            endingLabel.fontSize = 48;
            endingLabel.alignment = TextAlignmentOptions.Center;
            endingLabel.color = new Color(1f, 1f, 1f, 0f);
            RectTransform rt = endingLabel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        endingLabel.gameObject.SetActive(false);
    }
}