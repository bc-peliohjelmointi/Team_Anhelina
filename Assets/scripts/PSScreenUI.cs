using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PSScreenUI : MonoBehaviour
{
    public Canvas canvas;
    public Font textFont;

    [Header("Colors")]
    public Color textColor = Color.green;
    public Color highlightColor = new Color(0f, 1f, 0f, 0.2f);
    public Color checkmarkColor = Color.green;
    public Color errorColor = Color.red;

    [Header("Layout")]
    public float textStartY = -20f;
    public float textSpacing = 30f;
    public float textSize = 14f;

    private List<Text> menuTexts = new List<Text>();
    private Image selectionHighlight;
    private List<Image> checkmarks = new List<Image>();
    private Text errorText;

    void Start()
    {
        CreateCanvas();
        CreateMenuUI();
    }

    void CreateCanvas()
    {
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("PS_MenuCanvas");
            canvasObj.transform.SetParent(transform);

            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            canvasObj.AddComponent<GraphicRaycaster>();

            RectTransform rectTransform = canvasObj.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, 0, -0.01f);
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
            rectTransform.sizeDelta = new Vector2(200, 150);
        }

        canvas.gameObject.SetActive(false);
    }

    void CreateMenuUI()
    {
        string[] menuOptions = new string[]
        {
            "Turn on first episode",
            "Turn on second episode",
            "Turn on third episode"
        };

        for (int i = 0; i < menuOptions.Length; i++)
        {
            GameObject textObj = new GameObject("Episode" + (i + 1) + "Text");
            textObj.transform.SetParent(canvas.transform);

            Text text = textObj.AddComponent<Text>();
            text.text = menuOptions[i];
            text.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = (int)textSize;
            text.color = textColor;
            text.alignment = TextAnchor.MiddleLeft;

            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(10, textStartY - (i * textSpacing));
            rectTransform.sizeDelta = new Vector2(180, 30);

            menuTexts.Add(text);

            GameObject checkmarkObj = new GameObject("Checkmark" + (i + 1));
            checkmarkObj.transform.SetParent(canvas.transform);

            Image checkmark = checkmarkObj.AddComponent<Image>();
            checkmark.color = checkmarkColor;

            RectTransform checkRect = checkmarkObj.GetComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0, 1);
            checkRect.anchorMax = new Vector2(0, 1);
            checkRect.pivot = new Vector2(0, 1);
            checkRect.anchoredPosition = new Vector2(170, textStartY - (i * textSpacing) - 5);
            checkRect.sizeDelta = new Vector2(20, 20);

            checkmarkObj.SetActive(false);
            checkmarks.Add(checkmark);
        }

        GameObject highlightObj = new GameObject("SelectionHighlight");
        highlightObj.transform.SetParent(canvas.transform);

        selectionHighlight = highlightObj.AddComponent<Image>();
        selectionHighlight.color = highlightColor;

        RectTransform highlightRect = highlightObj.GetComponent<RectTransform>();
        highlightRect.anchorMin = new Vector2(0, 1);
        highlightRect.anchorMax = new Vector2(0, 1);
        highlightRect.pivot = new Vector2(0, 1);
        highlightRect.anchoredPosition = new Vector2(10, textStartY);
        highlightRect.sizeDelta = new Vector2(180, 30);

        GameObject errorObj = new GameObject("ErrorText");
        errorObj.transform.SetParent(canvas.transform);

        errorText = errorObj.AddComponent<Text>();
        errorText.text = "TURN ON TV";
        errorText.font = textFont != null ? textFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        errorText.fontSize = 20;
        errorText.color = errorColor;
        errorText.alignment = TextAnchor.MiddleCenter;
        errorText.fontStyle = FontStyle.Bold;

        RectTransform errorRect = errorObj.GetComponent<RectTransform>();
        errorRect.anchorMin = new Vector2(0.5f, 0.5f);
        errorRect.anchorMax = new Vector2(0.5f, 0.5f);
        errorRect.pivot = new Vector2(0.5f, 0.5f);
        errorRect.anchoredPosition = Vector2.zero;
        errorRect.sizeDelta = new Vector2(180, 40);

        errorObj.SetActive(false);
    }

    public void ShowMenu()
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(true);
        }
    }

    public void HideMenu()
    {
        if (canvas != null)
        {
            canvas.gameObject.SetActive(false);
        }
    }

    public void UpdateSelection(int index)
    {
        if (selectionHighlight != null && index >= 0 && index < menuTexts.Count)
        {
            RectTransform highlightRect = selectionHighlight.GetComponent<RectTransform>();
            highlightRect.anchoredPosition = new Vector2(10, textStartY - (index * textSpacing));
        }

        for (int i = 0; i < menuTexts.Count; i++)
        {
            if (menuTexts[i] != null)
            {
                menuTexts[i].fontStyle = (i == index) ? FontStyle.Bold : FontStyle.Normal;
            }
        }
    }

    public void ShowCheckmark(int index)
    {
        if (index >= 0 && index < checkmarks.Count && checkmarks[index] != null)
        {
            checkmarks[index].gameObject.SetActive(true);
        }
    }

    public void ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);
        }
    }

    public void HideError()
    {
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    public int GetMenuCount()
    {
        return menuTexts.Count;
    }
}