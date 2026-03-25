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
    public Renderer leverRenderer;
    public Color highlightColor = new Color(0, 1, 1, 1);
    public float outlineWidth = 0.015f;

    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool isLightGreen = false;
    private Material lampMaterial;
    private Material[] originalMaterials;
    private Material[] highlightMaterials;

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

        if (leverRenderer != null)
        {
            originalMaterials = leverRenderer.materials;
        }

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
        if (lampMaterial != null)
        {
            Destroy(lampMaterial);
        }
        RemoveHighlightMaterials();
    }
}