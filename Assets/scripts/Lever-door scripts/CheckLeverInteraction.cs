using UnityEngine;

public class CheckLeverInteraction : MonoBehaviour
{
    [Header("Puzzle Level")]
    public PuzzleLevel puzzleLevel;

    [Header("Lever")]
    public Transform leverHandle;
    public Vector3 downRotation = new Vector3(45, 0, 0);
    public Vector3 upRotation = new Vector3(-45, 0, 0);
    public float leverSpeed = 5f;
    public float returnDelay = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pullSound;
    public float soundVolume = 0.5f;

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 3f;
    public Renderer leverRenderer;

    private bool isChecking = false;
    private float returnTimer = 0f;
    private Quaternion targetRotation;
    private Material leverMaterial;
    private Material originalMaterial;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        targetRotation = Quaternion.Euler(downRotation);
        if (leverHandle != null)
        {
            leverHandle.localRotation = targetRotation;
        }

        if (leverRenderer != null)
        {
            leverMaterial = leverRenderer.material;
            originalMaterial = new Material(leverMaterial);
        }
    }

    void Update()
    {
        if (leverHandle != null)
        {
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation,
                targetRotation,
                Time.deltaTime * leverSpeed
            );
        }

        if (isChecking)
        {
            returnTimer -= Time.deltaTime;
            if (returnTimer <= 0f)
            {
                targetRotation = Quaternion.Euler(downRotation);
                isChecking = false;
            }
        }
    }

    public void Pull()
    {
        if (isChecking) return;
        if (puzzleLevel == null) return;

        isChecking = true;
        returnTimer = returnDelay;
        targetRotation = Quaternion.Euler(upRotation);

        if (pullSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pullSound, soundVolume);
        }

        puzzleLevel.CheckCombination();
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