using UnityEngine;
public class MainControlPanel : MonoBehaviour
{
    public PuzzleLevel1 puzzleLevel1;
    public PuzzleLevel2 puzzleLevel2;
    public PuzzleLevel3 puzzleLevel3;
    public Light light1, light2, light3, light4;
    public Renderer renderer1, renderer2, renderer3, renderer4;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public string emissionProperty = "_EmissionColor";
    private Material mat1, mat2, mat3, mat4;
    private bool solved1, solved2, solved3, allSolved;

    void Start()
    {
        if (renderer1 != null) mat1 = renderer1.material;
        if (renderer2 != null) mat2 = renderer2.material;
        if (renderer3 != null) mat3 = renderer3.material;
        if (renderer4 != null) mat4 = renderer4.material;
        RefreshLights();
    }

    public void NotifyLevelSolved()
    {
        solved1 = puzzleLevel1 != null && puzzleLevel1.IsSolved();
        solved2 = puzzleLevel2 != null && puzzleLevel2.IsSolved();
        solved3 = puzzleLevel3 != null && puzzleLevel3.IsSolved();
        allSolved = solved1 && solved2 && solved3;
        RefreshLights();
    }

    void RefreshLights()
    {
        SetLight(light1, mat1, solved1);
        SetLight(light2, mat2, solved2);
        SetLight(light3, mat3, solved3);
        SetLight(light4, mat4, allSolved);
    }

    void SetLight(Light l, Material mat, bool green)
    {
        Color c = green ? greenColor : redColor;
        if (l != null) { l.color = c; l.enabled = true; }
        if (mat != null) { mat.EnableKeyword("_EMISSION"); mat.SetColor(emissionProperty, c * 2f); mat.color = c; }
    }

    public bool CanOpenDoor() => allSolved;

    void OnDestroy()
    {
        if (mat1 != null) Destroy(mat1); if (mat2 != null) Destroy(mat2);
        if (mat3 != null) Destroy(mat3); if (mat4 != null) Destroy(mat4);
    }
}