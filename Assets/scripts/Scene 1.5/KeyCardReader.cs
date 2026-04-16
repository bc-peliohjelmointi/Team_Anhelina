using DG.Tweening.Core.Easing;
using UnityEngine;
// card reader device, two of these in the scene
// AlarmRoom reader: stops alarm and opens alarm room door
// ExitDoor reader: opens the final exit door
// only shows glow and prompt if player actually has the card
// requires Unity 2022 for FindFirstObjectByType
public class KeyCardReader : MonoBehaviour
{
    public enum ReaderType { AlarmRoom, ExitDoor }
    // set this to AlarmRoom or ExitDoor in inspector
    public ReaderType readerType;
    public float interactionDistance = 2f;
    public KeyCode useKey = KeyCode.E;
    // canvas prompt "E - Swipe card"
    public GameObject interactionPrompt;
    // the door this reader controls
    public DoorController controlledDoor;
    // only needed for AlarmRoom reader
    public AlarmSystem alarmSystem;
    public GameFlowManager gameFlowManager;
    public AudioSource audioSource;
    public AudioClip successSound;
    // the small LED indicator on the reader device
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
        // starts red, goes green after successful swipe
        SetIndicator(idleColor);
    }

    void Update()
    {
        // exit door reader does nothing after its been used
        if (hasBeenUsed && readerType == ReaderType.ExitDoor) return;

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
            // only glow if player actually has the card
            if (aura != null) aura.SetGlow(isNearby && hasCard);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby && hasCard);
        }

        if (isNearby && hasCard && Input.GetKeyDown(useKey)) UseCard();
    }

    void UseCard()
    {
        if (successSound != null) audioSource.PlayOneShot(successSound);
        SetIndicator(successColor);

        if (readerType == ReaderType.AlarmRoom)
        {
            // stop alarm and unlock the room door
            if (alarmSystem != null) alarmSystem.StopAlarm();
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
            // open the final exit door
            hasBeenUsed = true;
            if (controlledDoor != null) controlledDoor.OpenDoor();
            if (gameFlowManager != null) gameFlowManager.OnExitUnlocked();
            if (TaskManager.Instance != null)
                TaskManager.Instance.CompleteTask(TaskManager.Instance.task_SwipeExit);
        }

        if (interactionPrompt != null) interactionPrompt.SetActive(false);
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