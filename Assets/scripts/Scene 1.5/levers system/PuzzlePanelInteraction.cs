using UnityEngine;
using System.Collections;
public class PuzzlePanelInteraction : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Transform playerCamera;
    [Header("Camera")]
    public Transform cameraTargetPosition;
    public float cameraSpeed = 3f;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactionPrompt;

    [Header("Raycast")]
    public float rayDistance = 10f;
    public LayerMask leverLayer = -1;

    [Header("Cursor")]
    public bool showCursor = true;
    public int cursorSize = 8;
    public Color cursorColor = Color.white;
    public float mouseSensitivity = 2f;
    public float maxVerticalAngle = 30f;
    public float maxHorizontalAngle = 40f;

    [Header("Puzzle Levels")]
    public PuzzleLevel1 puzzleLevel1;
    public PuzzleLevel2 puzzleLevel2;
    public PuzzleLevel3 puzzleLevel3;

    [Header("Exit Key")]
    public KeyCode exitKey = KeyCode.E;

    private bool isInPuzzleMode = false;
    private bool canInteract = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Quaternion targetCameraRotation;
    private float currentVerticalAngle = 0f;
    private float currentHorizontalAngle = 0f;
    private Texture2D cursorTexture;
    private GameObject currentHighlighted;
    private PlayerMovement playerMovement;
    private CheckLeverComponent currentCheckLever;
    private Coroutine moveCamCoroutine;

    void Start()
    {
        cursorTexture = new Texture2D(1, 1);
        cursorTexture.SetPixel(0, 0, cursorColor);
        cursorTexture.Apply();
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (player != null) playerMovement = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (!isInPuzzleMode)
            CheckProximity();
        else
            HandlePuzzleMode();
    }

    void CheckProximity()
    {
        if (player == null) return;
        float dist = Vector3.Distance(player.position, transform.position);
        bool near = dist <= interactionDistance;
        if (interactionPrompt != null) interactionPrompt.SetActive(near);
        if (near && Input.GetKeyDown(interactKey)) EnterPuzzleMode();
    }

    void HandlePuzzleMode()
    {
        if (!canInteract) return;
        HandleCameraRotation();
        CheckLeverHighlight();

        if (Input.GetMouseButtonDown(0)) ClickLever();
        if (Input.GetMouseButtonUp(0)) ReleaseLever();
        if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(KeyCode.Escape)) ExitPuzzleMode();
    }

    void HandleCameraRotation()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentHorizontalAngle = Mathf.Clamp(currentHorizontalAngle + mx, -maxHorizontalAngle, maxHorizontalAngle);
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle - my, -maxVerticalAngle, maxVerticalAngle);
        playerCamera.rotation = targetCameraRotation
            * Quaternion.AngleAxis(currentHorizontalAngle, Vector3.up)
            * Quaternion.AngleAxis(currentVerticalAngle, Vector3.right);
    }

    void CheckLeverHighlight()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, leverLayer))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj != currentHighlighted)
            {
                RemoveHighlight();
                currentHighlighted = obj;
                ApplyHighlight();
            }
        }
        else RemoveHighlight();
    }

    void ApplyHighlight()
    {
        if (currentHighlighted == null) return;
        currentHighlighted.GetComponent<Lever>()?.Highlight(true);
        currentHighlighted.GetComponent<DoubleLever>()?.Highlight(true);
        currentHighlighted.GetComponent<CheckLeverComponent>()?.Highlight(true);
        currentHighlighted.GetComponentInParent<PuzzleLevel1>()?.HighlightCheckLever(true);
        currentHighlighted.GetComponentInParent<PuzzleLevel2>()?.HighlightCheckLever(true);
        currentHighlighted.GetComponentInParent<PuzzleLevel3>()?.HighlightCheckLever(true);
    }

    void RemoveHighlight()
    {
        if (currentHighlighted == null) return;
        currentHighlighted.GetComponent<Lever>()?.Highlight(false);
        currentHighlighted.GetComponent<DoubleLever>()?.Highlight(false);
        currentHighlighted.GetComponent<CheckLeverComponent>()?.Highlight(false);
        currentHighlighted.GetComponentInParent<PuzzleLevel1>()?.HighlightCheckLever(false);
        currentHighlighted.GetComponentInParent<PuzzleLevel2>()?.HighlightCheckLever(false);
        currentHighlighted.GetComponentInParent<PuzzleLevel3>()?.HighlightCheckLever(false);
        currentHighlighted = null;
    }

    void ClickLever()
    {
        if (currentHighlighted == null) return;

        Lever lever = currentHighlighted.GetComponent<Lever>();
        if (lever != null) { lever.Toggle(); return; }

        DoubleLever dl = currentHighlighted.GetComponent<DoubleLever>();
        if (dl != null) { dl.Toggle(); return; }

        CheckLeverComponent clc = currentHighlighted.GetComponent<CheckLeverComponent>();
        if (clc != null) { currentCheckLever = clc; clc.PullLever(); return; }

        PuzzleLevel1 pl1 = currentHighlighted.GetComponentInParent<PuzzleLevel1>();
        if (pl1 != null) { pl1.PullCheckLever(); return; }

        PuzzleLevel2 pl2 = currentHighlighted.GetComponentInParent<PuzzleLevel2>();
        if (pl2 != null) { pl2.PullCheckLever(); return; }

        PuzzleLevel3 pl3 = currentHighlighted.GetComponentInParent<PuzzleLevel3>();
        if (pl3 != null) { pl3.PullCheckLever(); return; }
    }

    void ReleaseLever()
    {
        if (currentCheckLever != null) { currentCheckLever.ReleaseLever(); currentCheckLever = null; return; }
        puzzleLevel1?.ReleaseCheckLever();
        puzzleLevel2?.ReleaseCheckLever();
        puzzleLevel3?.ReleaseCheckLever();
    }

    public void EnterPuzzleMode()
    {
        isInPuzzleMode = true;
        canInteract = false;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (playerMovement != null) playerMovement.LockControl();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (moveCamCoroutine != null) StopCoroutine(moveCamCoroutine);
        moveCamCoroutine = StartCoroutine(MoveCameraToTarget());
    }

    public void ExitPuzzleMode()
    {
        isInPuzzleMode = false;
        canInteract = false;
        RemoveHighlight();
        ReleaseLever();
        currentVerticalAngle = 0f;
        currentHorizontalAngle = 0f;
        if (playerMovement != null) playerMovement.UnlockControl();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (moveCamCoroutine != null) StopCoroutine(moveCamCoroutine);
        moveCamCoroutine = StartCoroutine(MoveCameraBack());
    }

    IEnumerator MoveCameraToTarget()
    {
        if (playerCamera == null || cameraTargetPosition == null) yield break;
        originalCameraPosition = playerCamera.position;
        originalCameraRotation = playerCamera.rotation;
        float elapsed = 0f;
        float duration = 1f / cameraSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            playerCamera.position = Vector3.Lerp(originalCameraPosition, cameraTargetPosition.position, t);
            playerCamera.rotation = Quaternion.Slerp(originalCameraRotation, cameraTargetPosition.rotation, t);
            yield return null;
        }
        playerCamera.position = cameraTargetPosition.position;
        playerCamera.rotation = cameraTargetPosition.rotation;
        targetCameraRotation = cameraTargetPosition.rotation;
        currentVerticalAngle = 0f;
        currentHorizontalAngle = 0f;
        canInteract = true;
    }

    IEnumerator MoveCameraBack()
    {
        if (playerCamera == null) yield break;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;
        float elapsed = 0f;
        float duration = 1f / cameraSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            playerCamera.position = Vector3.Lerp(startPos, originalCameraPosition, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, originalCameraRotation, t);
            yield return null;
        }
        playerCamera.position = originalCameraPosition;
        playerCamera.rotation = originalCameraRotation;
    }

    void OnGUI()
    {
        if (!isInPuzzleMode || !showCursor || !canInteract) return;
        float x = (Screen.width - cursorSize) * 0.5f;
        float y = (Screen.height - cursorSize) * 0.5f;
        GUI.DrawTexture(new Rect(x, y, cursorSize, cursorSize), cursorTexture);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        if (cameraTargetPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cameraTargetPosition.position, 0.15f);
            Gizmos.DrawRay(cameraTargetPosition.position, cameraTargetPosition.forward * 0.5f);
        }
    }

    void OnDisable()
    {
        if (isInPuzzleMode) ExitPuzzleMode();
    }
}