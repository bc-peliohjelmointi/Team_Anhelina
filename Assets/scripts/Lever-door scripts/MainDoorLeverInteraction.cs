using UnityEngine;

public class MainDoorLeverInteraction : MonoBehaviour
{
    [Header("Door Lever")]
    public MainDoorLever doorLever;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public GameObject interactionPrompt;

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 3f;
    public Renderer leverRenderer;

    private bool isNearby = false;
    private Material leverMaterial;
    private Material originalMaterial;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (leverRenderer != null)
        {
            leverMaterial = leverRenderer.material;
            originalMaterial = new Material(leverMaterial);
        }
    }

    void Update()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (!isNearby)
                {
                    isNearby = true;
                    Highlight(true);

                    if (interactionPrompt != null)
                    {
                        interactionPrompt.SetActive(true);
                    }
                }

                if (Input.GetKeyDown(interactKey))
                {
                    if (doorLever != null)
                    {
                        doorLever.PullLever();
                    }
                }
            }
            else
            {
                if (isNearby)
                {
                    isNearby = false;
                    Highlight(false);

                    if (interactionPrompt != null)
                    {
                        interactionPrompt.SetActive(false);
                    }
                }
            }
        }
        else
        {
            if (isNearby)
            {
                isNearby = false;
                Highlight(false);

                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }
    }

    void Highlight(bool enable)
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