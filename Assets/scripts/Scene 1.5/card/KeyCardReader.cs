using UnityEngine;
public class KeyCardReader : MonoBehaviour
{
    public enum ReaderType { AlarmRoom, ExitDoor }
    public ReaderType readerType;
    public float interactionDistance = 2f;
    public KeyCode useKey = KeyCode.E;
    public GameObject interactionPrompt;
    public DoorController controlledDoor;
    public AlarmSystem alarmSystem;
    public GameFlowManager gameFlowManager;
    public AudioSource audioSource;
    public AudioClip successSound;
    public Renderer indicatorRenderer;
    public Color idleColor = Color.red;
    public Color successColor = Color.green;
    public string emissionProperty = "_EmissionColor";
    private bool isNearby = false, hasBeenUsed = false;
    private AuraHighlight aura;
    private BoardController boardController;

    void Start()
    {
        aura = GetComponent<AuraHighlight>();
        boardController = FindObjectOfType<BoardController>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        SetIndicator(idleColor);
    }

    void Update()
    {
        if (hasBeenUsed && readerType == ReaderType.ExitDoor) return;
        bool hasCard = boardController != null && boardController.HasCard();
        Camera cam = Camera.main; if (cam == null) return;
        bool lookingAt = Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out RaycastHit hit, interactionDistance)
                         && hit.collider.gameObject == gameObject;
        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
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
            if (alarmSystem != null) alarmSystem.StopAlarm();
            if (controlledDoor != null) controlledDoor.OpenDoor();
            if (gameFlowManager != null) gameFlowManager.OnAlarmDeactivated();
            if (TaskManager.Instance != null) { TaskManager.Instance.CompleteTask(TaskManager.Instance.task_SwipeAlarm); TaskManager.Instance.AddTask(TaskManager.Instance.task_SwipeExit); }
        }
        else
        {
            hasBeenUsed = true;
            if (controlledDoor != null) controlledDoor.OpenDoor();
            if (gameFlowManager != null) gameFlowManager.OnExitUnlocked();
            if (TaskManager.Instance != null) TaskManager.Instance.CompleteTask(TaskManager.Instance.task_SwipeExit);
        }
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void SetIndicator(Color color)
    {
        if (indicatorRenderer == null) return;
        Material mat = indicatorRenderer.material;
        mat.EnableKeyword("_EMISSION"); mat.SetColor(emissionProperty, color * 2f); mat.color = color;
    }
}