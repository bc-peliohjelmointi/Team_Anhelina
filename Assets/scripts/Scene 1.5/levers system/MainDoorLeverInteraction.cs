using UnityEngine;
public class MainDoorLeverInteraction : MonoBehaviour
{
    public MainDoorLever doorLever;
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactionPrompt;
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
        bool lookingAt = Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out RaycastHit hit, interactionDistance)
                         && hit.collider.gameObject == gameObject;
        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (auraHighlight != null) auraHighlight.SetGlow(isNearby);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }
        if (isNearby && Input.GetKeyDown(interactKey) && doorLever != null) doorLever.PullLever();
    }
}