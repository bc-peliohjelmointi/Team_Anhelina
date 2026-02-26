using UnityEngine;
using System.Collections;

public class PSMenuObject : MonoBehaviour
{
    public int menuIndex = 0;
    public string fullText = "Turn on first episode";

    private Renderer objectRenderer;
    private Material materialInstance;
    private TextMesh textMesh;
    private Color normalColor = Color.green;
    private Color highlightColor = Color.white;

    void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            materialInstance = new Material(objectRenderer.sharedMaterial);
            objectRenderer.material = materialInstance;
        }

        textMesh = GetComponentInChildren<TextMesh>();
        if (textMesh != null && string.IsNullOrEmpty(fullText))
        {
            fullText = textMesh.text;
        }

        ResetText();
    }

    public IEnumerator TypeText(float delay)
    {
        if (textMesh == null) yield break;

        textMesh.text = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            textMesh.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(delay);
        }
    }

    public void ShowFullText()
    {
        if (textMesh != null)
        {
            textMesh.text = fullText;
        }
    }

    public void ResetText()
    {
        if (textMesh != null)
        {
            textMesh.text = "";
        }
    }

    public void Highlight()
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor("_Color", highlightColor);
        }
        if (textMesh != null)
        {
            textMesh.color = highlightColor;
        }
    }

    public void Unhighlight()
    {
        if (materialInstance != null)
        {
            materialInstance.SetColor("_Color", normalColor);
        }
        if (textMesh != null)
        {
            textMesh.color = normalColor;
        }
    }
}