using UnityEngine;
// this is the validation lever for one row of the puzzle
// player holds it down to check if the combination is correct
// only fill the puzzleLevel field that matches this levers row
// PuzzlePanelInteraction calls PullLever on mouse down, ReleaseLever on mouse up
public class CheckLeverComponent : MonoBehaviour
{
    // renderer of this lever mesh, used for highlight outline
    public Renderer leverRenderer;
    // only assign one of these, the row this check lever validates
    public PuzzleLevel1 puzzleLevel1;
    public PuzzleLevel2 puzzleLevel2;
    public PuzzleLevel3 puzzleLevel3;
    public Color highlightColor = new Color(1f, 1f, 0f, 1f);
    public float outlineWidth = 0.015f;
    private Material[] originalMaterials;
    private Material[] highlightMaterials;
    private bool isPulling = false;

    void Start()
    {
        if (leverRenderer != null) originalMaterials = leverRenderer.materials;
    }

    // called by PuzzlePanelInteraction when player clicks and holds this lever
    public void PullLever()
    {
        if (isPulling) return;
        isPulling = true;
        if (puzzleLevel1 != null) puzzleLevel1.PullCheckLever();
        if (puzzleLevel2 != null) puzzleLevel2.PullCheckLever();
        if (puzzleLevel3 != null) puzzleLevel3.PullCheckLever();
    }

    // called when player releases mouse button
    public void ReleaseLever()
    {
        if (!isPulling) return;
        isPulling = false;
        if (puzzleLevel1 != null) puzzleLevel1.ReleaseCheckLever();
        if (puzzleLevel2 != null) puzzleLevel2.ReleaseCheckLever();
        if (puzzleLevel3 != null) puzzleLevel3.ReleaseCheckLever();
    }

    public void Highlight(bool enable)
    {
        if (leverRenderer == null || originalMaterials == null) return;
        if (enable) CreateHighlight(); else RemoveHighlight();
    }

    void CreateHighlight()
    {
        if (highlightMaterials != null) return;
        Shader s = Shader.Find("Custom/OutlineEdge");
        if (s == null) return;
        highlightMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++) highlightMaterials[i] = originalMaterials[i];
        Material m = new Material(s);
        m.SetColor("_OutlineColor", highlightColor);
        m.SetFloat("_OutlineWidth", outlineWidth);
        highlightMaterials[highlightMaterials.Length - 1] = m;
        leverRenderer.materials = highlightMaterials;
    }

    void RemoveHighlight()
    {
        if (highlightMaterials == null) return;
        leverRenderer.materials = originalMaterials;
        if (highlightMaterials.Length > originalMaterials.Length)
            Destroy(highlightMaterials[highlightMaterials.Length - 1]);
        highlightMaterials = null;
    }

    void OnDestroy() { RemoveHighlight(); }
}