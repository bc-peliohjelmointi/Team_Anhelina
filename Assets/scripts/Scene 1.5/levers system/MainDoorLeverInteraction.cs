using UnityEngine;
public class MainDoorLeverInteraction : MonoBehaviour
{
    [Header("References")]
    public MainDoorLever doorLever;
    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public GameObject interactionPrompt;

    [Header("Aura")]
    public AuraHighlight auraHighlight;

    private bool isNearby = false;

    void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (auraHighlight == null) auraHighlight = GetComponent<AuraHighlight>();
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        bool lookingAt = Physics.Raycast(ray, out RaycastHit hit, interactionDistance)
                         && hit.collider.gameObject == gameObject;

        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (auraHighlight != null) auraHighlight.SetGlow(isNearby);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }

        if (isNearby && Input.GetKeyDown(interactKey))
            if (doorLever != null) doorLever.PullLever();
    }
}