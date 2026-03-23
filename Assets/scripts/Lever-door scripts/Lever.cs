using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    public bool isUp = false;
    public Vector3 downRotation = new Vector3(45, 0, 0);
    public Vector3 upRotation = new Vector3(-45, 0, 0);
    public Vector3 downPosition = Vector3.zero;
    public Vector3 upPosition = Vector3.zero;
    public float switchSpeed = 5f;

    [Header("Light")]
    public Light lampLight;
    public Renderer lampRenderer;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    public string emissionProperty = "_EmissionColor";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip switchSound;
    public float soundVolume = 0.5f;

    [Header("Outline Highlight")]
    public MeshFilter leverMeshFilter;
    public Color highlightColor = Color.cyan;
    public float outlineWidth = 0.02f;

    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool isLightGreen = false;
    private Material lampMaterial;
    private GameObject outlineObject;

    void Start()
    {
        if (lampRenderer != null)
        {
            lampMaterial = lampRenderer.material;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        UpdateLeverState(true);
    }

    void Update()
    {
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            targetRotation,
            Time.deltaTime * switchSpeed
        );

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * switchSpeed
        );
    }

    public void Toggle()
    {
        isUp = !isUp;
        UpdateLeverState(false);

        if (switchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(switchSound, soundVolume);
        }
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
        Color currentColor = isLightGreen ? greenColor : redColor;

        if (lampLight != null)
        {
            lampLight.color = currentColor;
        }

        if (lampMaterial != null)
        {
            lampMaterial.EnableKeyword("_EMISSION");
            lampMaterial.SetColor(emissionProperty, currentColor * 2f);
            lampMaterial.color = currentColor;
        }
    }

    public void Highlight(bool enable)
    {
        if (enable)
        {
            CreateOutline();
        }
        else
        {
            RemoveOutline();
        }
    }

    void CreateOutline()
    {
        if (outlineObject != null || leverMeshFilter == null) return;

        outlineObject = new GameObject("LeverOutline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one * (1f + outlineWidth);

        MeshFilter outlineMeshFilter = outlineObject.AddComponent<MeshFilter>();
        outlineMeshFilter.mesh = leverMeshFilter.mesh;

        MeshRenderer outlineRenderer = outlineObject.AddComponent<MeshRenderer>();
        Material outlineMaterial = new Material(Shader.Find("Standard"));
        outlineMaterial.color = highlightColor;
        outlineMaterial.SetFloat("_Metallic", 0f);
        outlineMaterial.SetFloat("_Glossiness", 0.8f);
        outlineRenderer.material = outlineMaterial;
        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        outlineObject.layer = gameObject.layer;
    }

    void RemoveOutline()
    {
        if (outlineObject != null)
        {
            Destroy(outlineObject);
            outlineObject = null;
        }
    }

    void OnDestroy()
    {
        if (lampMaterial != null)
        {
            Destroy(lampMaterial);
        }
        RemoveOutline();
    }
}