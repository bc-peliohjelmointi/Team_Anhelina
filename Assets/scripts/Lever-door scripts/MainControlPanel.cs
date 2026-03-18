using UnityEngine;

public class MainControlPanel : MonoBehaviour
{
    [Header("Puzzle Levels")]
    public PuzzleLevel level1;
    public PuzzleLevel level2;
    public PuzzleLevel level3;

    [Header("Indicator Lights")]
    public Light light1;
    public Light light2;
    public Light light3;
    public Light light4;

    [Header("Indicator Renderers")]
    public Renderer renderer1;
    public Renderer renderer2;
    public Renderer renderer3;
    public Renderer renderer4;

    [Header("Colors")]
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public string emissionProperty = "_EmissionColor";

    private Material mat1, mat2, mat3, mat4;
    private bool allSolved = false;

    void Start()
    {
        if (renderer1 != null) mat1 = renderer1.material;
        if (renderer2 != null) mat2 = renderer2.material;
        if (renderer3 != null) mat3 = renderer3.material;
        if (renderer4 != null) mat4 = renderer4.material;

        UpdateAllLights();
    }

    void Update()
    {
        UpdateAllLights();
    }

    void UpdateAllLights()
    {
        bool solved1 = level1 != null && level1.IsSolved();
        bool solved2 = level2 != null && level2.IsSolved();
        bool solved3 = level3 != null && level3.IsSolved();

        SetLight(light1, mat1, solved1);
        SetLight(light2, mat2, solved2);
        SetLight(light3, mat3, solved3);

        bool newAllSolved = solved1 && solved2 && solved3;

        if (newAllSolved != allSolved)
        {
            allSolved = newAllSolved;
            SetLight(light4, mat4, allSolved);
        }
    }

    void SetLight(Light lightComp, Material mat, bool isGreen)
    {
        Color color = isGreen ? greenColor : redColor;

        if (lightComp != null)
        {
            lightComp.color = color;
            lightComp.enabled = true;
        }

        if (mat != null)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor(emissionProperty, color * 2f);
            mat.color = color;
        }
    }

    public bool CanOpenDoor()
    {
        return allSolved;
    }

    void OnDestroy()
    {
        if (mat1 != null) Destroy(mat1);
        if (mat2 != null) Destroy(mat2);
        if (mat3 != null) Destroy(mat3);
        if (mat4 != null) Destroy(mat4);
    }
}