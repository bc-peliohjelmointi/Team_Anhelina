using UnityEngine;

public class DoubleLever : MonoBehaviour
{
    [Header("Lever Settings")]
    public bool isUp = false;
    public Vector3 downRotation = new Vector3(45, 0, 0);
    public Vector3 upRotation = new Vector3(-45, 0, 0);
    public Vector3 downPosition = Vector3.zero;
    public Vector3 upPosition = Vector3.zero;
    public float switchSpeed = 5f;

    [Header("Lights")]
    public Light topLampLight;
    public Light bottomLampLight;
    public Renderer topLampRenderer;
    public Renderer bottomLampRenderer;
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
    private Vector3 targetPosition;
    private bool topLightGreen = false;
    private bool bottomLightGreen = false;
    private Material topLampMaterial;
    private Material bottomLampMaterial;

    void Start()
    {
        if (topLampRenderer != null)
        {
            topLampMaterial = topLampRenderer.material;
        }

        if (bottomLampRenderer != null)
        {
            bottomLampMaterial = bottomLampRenderer.material;
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

    public void SetTopLightGreen(bool green)
    {
        topLightGreen = green;
        UpdateLights();
    }

    public void SetBottomLightGreen(bool green)
    {
        bottomLightGreen = green;
        UpdateLights();
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

        UpdateLights();
    }

    void UpdateLights()
    {
        Color topColor = topLightGreen ? greenColor : redColor;
        Color bottomColor = bottomLightGreen ? greenColor : redColor;

        if (topLampLight != null)
        {
            topLampLight.color = topColor;
        }

        if (bottomLampLight != null)
        {
            bottomLampLight.color = bottomColor;
        }

        if (topLampMaterial != null)
        {
            topLampMaterial.EnableKeyword("_EMISSION");
            topLampMaterial.SetColor(emissionProperty, topColor * 2f);
            topLampMaterial.color = topColor;
        }

        if (bottomLampMaterial != null)
        {
            bottomLampMaterial.EnableKeyword("_EMISSION");
            bottomLampMaterial.SetColor(emissionProperty, bottomColor * 2f);
            bottomLampMaterial.color = bottomColor;
        }
    }

    public void Highlight(bool enable)
    {
        if (enable)
        {
            if (topLampMaterial != null)
            {
                Color topColor = topLightGreen ? greenColor : redColor;
                topLampMaterial.SetColor(emissionProperty, topColor * highlightIntensity);
            }

            if (bottomLampMaterial != null)
            {
                Color bottomColor = bottomLightGreen ? greenColor : redColor;
                bottomLampMaterial.SetColor(emissionProperty, bottomColor * highlightIntensity);
            }
        }
        else
        {
            UpdateLights();
        }
    }

    void OnDestroy()
    {
        if (topLampMaterial != null)
        {
            Destroy(topLampMaterial);
        }
        if (bottomLampMaterial != null)
        {
            Destroy(bottomLampMaterial);
        }
    }
}