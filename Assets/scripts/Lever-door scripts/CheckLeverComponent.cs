using UnityEngine;

public class CheckLeverComponent : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer leverRenderer;

    [Header("Highlight")]
    public Color highlightColor = new Color(1, 1, 0, 1);
    public float outlineWidth = 0.015f;

    private Material[] originalMaterials;
    private Material[] highlightMaterials;

    void Start()
    {
        if (leverRenderer != null)
        {
            originalMaterials = leverRenderer.materials;
        }
    }

    public void Highlight(bool enable)
    {
        if (leverRenderer == null || originalMaterials == null) return;

        if (enable)
        {
            CreateHighlightMaterials();
        }
        else
        {
            RemoveHighlightMaterials();
        }
    }

    void CreateHighlightMaterials()
    {
        if (highlightMaterials != null) return;

        Shader outlineShader = Shader.Find("Custom/OutlineEdge");
        if (outlineShader == null)
        {
            Debug.LogWarning("OutlineEdge shader not found!");
            return;
        }

        highlightMaterials = new Material[originalMaterials.Length + 1];

        for (int i = 0; i < originalMaterials.Length; i++)
        {
            highlightMaterials[i] = originalMaterials[i];
        }

        Material outlineMat = new Material(outlineShader);
        outlineMat.SetColor("_OutlineColor", highlightColor);
        outlineMat.SetFloat("_OutlineWidth", outlineWidth);
        highlightMaterials[highlightMaterials.Length - 1] = outlineMat;

        leverRenderer.materials = highlightMaterials;
    }

    void RemoveHighlightMaterials()
    {
        if (highlightMaterials == null) return;

        leverRenderer.materials = originalMaterials;

        if (highlightMaterials.Length > originalMaterials.Length)
        {
            Destroy(highlightMaterials[highlightMaterials.Length - 1]);
        }

        highlightMaterials = null;
    }

    void OnDestroy()
    {
        RemoveHighlightMaterials();
    }
}