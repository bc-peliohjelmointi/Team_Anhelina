using UnityEngine;

public class CheckLeverInteraction : MonoBehaviour
{
    [Header("Puzzle Level")]
    public PuzzleLevel puzzleLevel;

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 3f;
    public Renderer leverRenderer;

    private Material leverMaterial;
    private Material originalMaterial;

    void Start()
    {
        if (leverRenderer != null)
        {
            leverMaterial = leverRenderer.material;
            originalMaterial = new Material(leverMaterial);
        }
    }

    public void Pull()
    {
        if (puzzleLevel != null)
        {
            puzzleLevel.PullCheckLever();
        }
    }

    public void Release()
    {
        if (puzzleLevel != null)
        {
            puzzleLevel.ReleaseCheckLever();
        }
    }

    public void Highlight(bool enable)
    {
        if (leverMaterial == null) return;

        if (enable)
        {
            leverMaterial.EnableKeyword("_EMISSION");
            leverMaterial.SetColor("_EmissionColor", highlightColor * highlightIntensity);
        }
        else
        {
            leverMaterial.DisableKeyword("_EMISSION");
            leverMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    void OnDestroy()
    {
        if (leverMaterial != null && leverMaterial != originalMaterial)
        {
            Destroy(leverMaterial);
        }
    }
}