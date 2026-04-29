using UnityEngine;
// handles the E key interaction with the big lever
// uses raycast from camera, shows aura glow and prompt when player looks at it
// needs both MainDoorLever and AuraHighlight on the same object
public class MainDoorLeverInteraction : MonoBehaviour
{
    // the lever script on this same object
    public MainDoorLever doorLever;
    // how close player needs to be to interact
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    // the "E - Pull lever" canvas prompt
    public GameObject interactionPrompt;
    // aura on this same object, can leave empty and it finds it automatically
    public AuraHighlight auraHighlight;
    private bool isNearby = false;

    void Start()
    {
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        // try to grab aura from this object if not assigned
        if (auraHighlight == null) auraHighlight = GetComponent<AuraHighlight>();
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        bool lookingAt = Physics.Raycast(
            new Ray(cam.transform.position, cam.transform.forward),
            out RaycastHit hit, interactionDistance)
            && hit.collider.gameObject == gameObject;

        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (auraHighlight != null) auraHighlight.SetGlow(isNearby);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }

        if (isNearby && Input.GetKeyDown(interactKey) && doorLever != null)
            doorLever.PullLever();
    }
}