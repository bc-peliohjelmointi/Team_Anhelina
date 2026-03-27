using UnityEngine;
public class Lever : MonoBehaviour
{
    [Header("State")]
    public bool isUp = false;
    [Header("Animation")]
    public Vector3 downRotation = new Vector3(45f, 0f, 0f);
    public Vector3 upRotation = new Vector3(-45f, 0f, 0f);
    public Vector3 downPosition = Vector3.zero;
    public Vector3 upPosition = Vector3.zero;
    public float switchSpeed = 5f;

    [Header("Lamp")]
    public Light lampLight;
    public Renderer lampRenderer;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public string emissionProperty = "_EmissionColor";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip switchSound;
    public float soundVolume = 0.5f;

    [Header("Aura")]
    public Renderer leverRenderer;
    public Color highlightColor = new Color(0f, 1f, 1f, 1f);
    public float outlineWidth = 0.015f;

    [Header("Owner")]
    public PuzzleLevel1 ownerLevel1;
    public PuzzleLevel2 ownerLevel2;
    public PuzzleLevel3 ownerLevel3;

    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool isLightGreen = false;
    private Material lampMaterial;
    private Material[] originalMaterials;
    private Material[] highlightMaterials;

    void Start()
    {
        if (lampRenderer != null) lampMaterial = lampRenderer.material;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        if (leverRenderer != null) originalMaterials = leverRenderer.materials;
        UpdateLeverState(true);
    }

    void Update()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * switchSpeed);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * switchSpeed);
    }

    public void Toggle()
    {
        isUp = !isUp;
        UpdateLeverState(false);
        if (switchSound != null) audioSource.PlayOneShot(switchSound, soundVolume);
        NotifyOwner();
    }

    void NotifyOwner()
    {
        if (ownerLevel1 != null) ownerLevel1.OnLeverChanged();
        if (ownerLevel2 != null) ownerLevel2.OnLeverChanged();
        if (ownerLevel3 != null) ownerLevel3.OnLeverChanged();
    }

    public void SetLightGreen(bool green)
    {
        isLightGreen = green;
        UpdateLight();
    }

    void UpdateLeverState(bool immediate)
    {
        targetRotation = Quaternion.Euler(isUp ? upRotation : downRotation);
        targetPosition = isUp ? upPosition : downPosition;
        if (immediate)
        {
            transform.localRotation = targetRotation;
            transform.localPosition = targetPosition;
        }
        UpdateLight();
    }

    void UpdateLight()
    {
        Color c = isLightGreen ? greenColor : redColor;
        if (lampLight != null) { lampLight.color = c; }
        if (lampMaterial != null)
        {
            lampMaterial.EnableKeyword("_EMISSION");
            lampMaterial.SetColor(emissionProperty, c * 2f);
            lampMaterial.color = c;
        }
    }

    public void Highlight(bool enable)
    {
        if (leverRenderer == null || originalMaterials == null) return;
        if (enable) CreateHighlight();
        else RemoveHighlight();
    }

    void CreateHighlight()
    {
        if (highlightMaterials != null) return;
        Shader s = Shader.Find("Custom/OutlineEdge");
        if (s == null) return;
        highlightMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++)
            highlightMaterials[i] = originalMaterials[i];
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

    void OnDestroy()
    {
        if (lampMaterial != null) Destroy(lampMaterial);
        RemoveHighlight();
    }
}