using UnityEngine;
public class KeyCard : MonoBehaviour
{
    public float interactionDistance = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public GameObject interactionPrompt;
    public GameObject cardViewModel;
    private bool isPickedUp = false, isNearby = false;
    private AuraHighlight aura;
    private BoardController boardController;

    void Start()
    {
        aura = GetComponent<AuraHighlight>();
        boardController = FindObjectOfType<BoardController>();
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (isPickedUp) return;
        Camera cam = Camera.main; if (cam == null) return;
        bool lookingAt = Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out RaycastHit hit, interactionDistance)
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
        if (boardController != null) boardController.OnCardPickedUp(cardViewModel);
        if (TaskManager.Instance != null) TaskManager.Instance.CompleteTask(TaskManager.Instance.task_TakeCard);
        gameObject.SetActive(false);
    }
}