using UnityEngine;
using System.Collections;
// office door opener using card swipe
// door transform moves from closedPoint to openPoint
// closedPoint and openPoint are empty GameObjects placed in the scene
// player swipes card at this reader ? door moves to open position
// only works after GameFlowManager.AlarmOff stage
// this replaces the KeyCardReader for the office exit door
public class OfficeDoorOpener : MonoBehaviour
{
    // ---- door movement ----
    // the door object that moves
    public Transform doorTransform;
    // empty GameObject at door closed position - place it where door starts
    public Transform closedPoint;
    // empty GameObject at door open position - place it where door ends up
    public Transform openPoint;
    // how fast door moves between points
    public float doorMoveSpeed = 1.5f;
    // easing curve for smooth movement
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // ---- interaction ----
    public float interactionDistance = 2f;
    public KeyCode useKey = KeyCode.E;
    // "E - ???????? ??????" prompt
    public GameObject interactionPrompt;
    public AuraHighlight auraHighlight;

    // ---- condition ----
    // door reader only activates after alarm is off
    public GameFlowManager gameFlowManager;

    // ---- audio ----
    public AudioSource audioSource;
    public AudioClip swipeSound;
    public AudioClip doorMoveSound;
    public AudioClip deniedSound;
    public float soundVolume = 0.6f;

    // ---- LED indicator ----
    public Renderer indicatorRenderer;
    public Color idleColor = Color.red;
    public Color successColor = Color.green;
    public string emissionProperty = "_EmissionColor";

    // ---- state ----
    private bool isOpen = false;
    private bool isMoving = false;
    private bool isNearby = false;
    private BoardController boardController;

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        boardController = Object.FindFirstObjectByType<BoardController>();
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        SetIndicator(idleColor);
        // snap door to closed position at start
        if (doorTransform != null && closedPoint != null)
        {
            doorTransform.position = closedPoint.position;
            doorTransform.rotation = closedPoint.rotation;
        }
    }

    void Update()
    {
        if (isOpen || isMoving) return;
        // only show interaction when game flow allows it
        bool canUse = gameFlowManager != null &&
            (gameFlowManager.currentStage == GameFlowManager.GameStage.AlarmOff ||
             gameFlowManager.currentStage == GameFlowManager.GameStage.ExitReady);
        if (!canUse) return;
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
            if (auraHighlight != null) auraHighlight.SetGlow(isNearby && hasCard);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby && hasCard);
        }
        if (isNearby && Input.GetKeyDown(useKey))
        {
            if (hasCard) OpenDoor();
            else if (deniedSound != null) audioSource.PlayOneShot(deniedSound, soundVolume);
        }
    }

    void OpenDoor()
    {
        if (isOpen || isMoving) return;
        if (swipeSound != null) audioSource.PlayOneShot(swipeSound, soundVolume);
        SetIndicator(successColor);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (auraHighlight != null) auraHighlight.SetGlow(false);
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.CompleteTask(TaskManager.Instance.task_SwipeAlarm);
            TaskManager.Instance.AddTask(TaskManager.Instance.task_SwipeExit);
        }
        StartCoroutine(MoveDoor());
    }

    IEnumerator MoveDoor()
    {
        isMoving = true;
        if (doorMoveSound != null) audioSource.PlayOneShot(doorMoveSound, soundVolume);
        Vector3 startPos = closedPoint != null ? closedPoint.position : doorTransform.position;
        Quaternion startRot = closedPoint != null ? closedPoint.rotation : doorTransform.rotation;
        Vector3 endPos = openPoint != null ? openPoint.position : doorTransform.position;
        Quaternion endRot = openPoint != null ? openPoint.rotation : doorTransform.rotation;
        float elapsed = 0f;
        float duration = 1f / doorMoveSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = moveCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
            if (doorTransform != null)
            {
                doorTransform.position = Vector3.Lerp(startPos, endPos, t);
                doorTransform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }
            yield return null;
        }
        if (doorTransform != null)
        {
            doorTransform.position = endPos;
            doorTransform.rotation = endRot;
        }
        isOpen = true;
        isMoving = false;
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