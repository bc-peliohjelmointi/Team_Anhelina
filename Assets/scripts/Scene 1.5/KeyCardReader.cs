using UnityEngine;
// card reader device - two in the scene
// OfficeDoor reader: lets player EXIT the office after solving the puzzle
//   placed near the office door on the INSIDE of the alarm room
//   opens the office door and advances game flow
// ExitScene reader: loads the next scene, placed at the final exit point
// only shows glow and prompt if player actually has the card
// requires Unity 2022 for FindFirstObjectByType
public class KeyCardReader : MonoBehaviour
{
    public enum ReaderType
    {
        // reader that opens the office exit door (letting player leave the alarm room)
        OfficeDoor,
        // reader at the very end that triggers scene transition
        ExitScene
    }

    public ReaderType readerType;
    public float interactionDistance = 2f;
    public KeyCode useKey = KeyCode.E;
    // "E - Провести картой" prompt
    public GameObject interactionPrompt;
    // the door this reader controls (OfficeDoor reader only)
    public DoorController controlledDoor;
    public GameFlowManager gameFlowManager;
    public AudioSource audioSource;
    public AudioClip successSound;
    // sound when player tries without having the card
    public AudioClip deniedSound;
    // LED on the reader body
    public Renderer indicatorRenderer;
    public Color idleColor = Color.red;
    public Color successColor = Color.green;
    public string emissionProperty = "_EmissionColor";

    private bool isNearby = false;
    private bool hasBeenUsed = false;
    private AuraHighlight aura;
    private BoardController boardController;

    void Start()
    {
        aura = GetComponent<AuraHighlight>();
        boardController = Object.FindFirstObjectByType<BoardController>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        SetIndicator(idleColor);
    }

    void Update()
    {
        if (hasBeenUsed) return;
        bool hasCard = boardController != null && boardController.HasCard();
        Camera cam = Camera.main;
        if (cam == null) return;
        bool lookingAt = Physics.Raycast(
            new Ray(cam.transform.position, cam.transform.forward),
            out RaycastHit hit, interactionDistance)
            && hit.collider.gameObject == gameObject;
        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (aura != null) aura.SetGlow(isNearby && hasCard);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby && hasCard);
        }
        if (isNearby && Input.GetKeyDown(useKey))
        {
            if (hasCard) UseCard();
            else if (deniedSound != null) audioSource.PlayOneShot(deniedSound);
        }
    }

    void UseCard()
    {
        hasBeenUsed = true;
        if (successSound != null) audioSource.PlayOneShot(successSound);
        SetIndicator(successColor);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (aura != null) aura.SetGlow(false);
        if (readerType == ReaderType.OfficeDoor)
        {
            // open alarm room door so player can leave
            if (controlledDoor != null) controlledDoor.OpenDoor();
            if (gameFlowManager != null) gameFlowManager.OnAlarmDeactivated();
            if (TaskManager.Instance != null)
            {
                TaskManager.Instance.CompleteTask(TaskManager.Instance.task_SwipeAlarm);
                TaskManager.Instance.AddTask(TaskManager.Instance.task_SwipeExit);
            }
        }
        else
        {
            // exit the scene entirely
            if (gameFlowManager != null) gameFlowManager.OnExitUnlocked();
            if (TaskManager.Instance != null)
                TaskManager.Instance.CompleteTask(TaskManager.Instance.task_SwipeExit);
        }
    }

    void SetIndicator(Color color)
    {
        if (indicatorRenderer == null) return;
        Material mat = indicatorRenderer.material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor(emissionProperty, color * 2f);
        mat.color = color;
    }
}