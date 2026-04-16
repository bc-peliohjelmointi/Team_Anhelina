using DG.Tweening.Core.Easing;
using UnityEngine;
// the card item that spawns inside the opened drawer
// player looks at it and presses E to pick it up
// after pickup tells BoardController so Tab can switch to card view
// uses FindFirstObjectByType - requires Unity 2022 or newer
public class KeyCard : MonoBehaviour
{
    // pickup range
    public float interactionDistance = 2f;
    public KeyCode pickupKey = KeyCode.E;
    // canvas above card with "E - Pick up" text
    public GameObject interactionPrompt;
    // optional 3D model shown when viewing card with Tab (can be null)
    public GameObject cardViewModel;
    private bool isPickedUp = false;
    private bool isNearby = false;
    private AuraHighlight aura;
    private BoardController boardController;

    void Start()
    {
        aura = GetComponent<AuraHighlight>();
        // find clipboard controller anywhere in the scene
        boardController = Object.FindFirstObjectByType<BoardController>();
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (isPickedUp) return;
        Camera cam = Camera.main;
        if (cam == null) return;

        bool lookingAt = Physics.Raycast(
            new Ray(cam.transform.position, cam.transform.forward),
            out RaycastHit hit, interactionDistance)
            && hit.collider.gameObject == gameObject;

        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (aura != null) aura.SetGlow(isNearby);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }

        if (isNearby && Input.GetKeyDown(pickupKey)) PickUp();
    }

    void PickUp()
    {
        isPickedUp = true;
        if (aura != null) aura.SetGlow(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        // tell clipboard we have a card now, Tab will switch to it
        if (boardController != null) boardController.OnCardPickedUp(cardViewModel);
        if (TaskManager.Instance != null)
            TaskManager.Instance.CompleteTask(TaskManager.Instance.task_TakeCard);
        // card is now "in inventory", hide from scene
        gameObject.SetActive(false);
    }
}