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

    [Header("UI (можно задать своё или создастся автоматически)")]
    public Canvas endingCanvas;
    public Image fadePanel;
    public TextMeshProUGUI endingLabel;

    private bool _triggered = false;

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

        // --- 1. Fade Out ---
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            fadePanel.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        fadePanel.color = Color.black;

        // --- 2. Надпись появляется ---
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

        // --- 3. Ждём ---
        yield return new WaitForSeconds(displayTime);

        // --- 4. Загружаем главное меню ---
        SceneManager.LoadScene(mainMenuSceneIndex);
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

        // Убеждаемся что FadePanel активен (Alpha=0, но объект включён)
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

        // EndingText скрыт до нужного момента
        endingLabel.gameObject.SetActive(false);
    }
}