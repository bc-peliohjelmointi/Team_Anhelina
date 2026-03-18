using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Lever Settings")]
    public bool isUp = false;
    public Vector3 downRotation = new Vector3(45, 0, 0);
    public Vector3 upRotation = new Vector3(-45, 0, 0);
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

    [Header("Highlight")]
    public Color highlightColor = Color.cyan;
    public float highlightIntensity = 2f;

    private Quaternion targetRotation;
    private bool isLightGreen = false;
    private Material lampMaterial;
    private Material originalMaterial;

    void Start()
    {
        if (lampRenderer != null)
        {
            lampMaterial = lampRenderer.material;
            originalMaterial = new Material(lampMaterial);
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

        if (immediate)
        {
            transform.localRotation = targetRotation;
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
        if (lampMaterial == null) return;

        if (enable)
        {
            lampMaterial.EnableKeyword("_EMISSION");
            Color currentColor = isLightGreen ? greenColor : redColor;
            lampMaterial.SetColor(emissionProperty, currentColor * highlightIntensity);
        }
        else
        {
            UpdateLight();
        }
    }

    void OnDestroy()
    {
        if (lampMaterial != null && lampMaterial != originalMaterial)
        {
            Destroy(lampMaterial);
        }
    }
}