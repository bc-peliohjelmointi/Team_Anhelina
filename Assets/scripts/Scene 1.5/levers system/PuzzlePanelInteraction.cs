using UnityEngine;
using System.Collections;
public class PuzzlePanelInteraction : MonoBehaviour
{
    public Transform player;
    public Transform playerCamera;
    public Transform cameraTargetPosition;
    public float cameraSpeed = 3f;
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactionPrompt;
    public float rayDistance = 10f;
    public LayerMask leverLayer = -1;
    public bool showCursor = true;
    public int cursorSize = 8;
    public Color cursorColor = Color.white;
    public float mouseSensitivity = 2f;
    public float maxVerticalAngle = 30f;
    public float maxHorizontalAngle = 40f;
    public PuzzleLevel1 puzzleLevel1;
    public PuzzleLevel2 puzzleLevel2;
    public PuzzleLevel3 puzzleLevel3;
    public KeyCode exitKey = KeyCode.E;
    private bool isInPuzzleMode = false, canInteract = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation, targetCameraRotation;
    private float currentVerticalAngle = 0f, currentHorizontalAngle = 0f;
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
        if (!isInPuzzleMode) CheckProximity();
        else HandlePuzzleMode();
    }

    void CheckProximity()
    {
        if (player == null) return;
        bool near = Vector3.Distance(player.position, transform.position) <= interactionDistance;
        if (interactionPrompt != null) interactionPrompt.SetActive(near);
        if (near && Input.GetKeyDown(interactKey)) EnterPuzzleMode();
    }

    void HandlePuzzleMode()
    {
        if (!canInteract) return;
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentHorizontalAngle = Mathf.Clamp(currentHorizontalAngle + mx, -maxHorizontalAngle, maxHorizontalAngle);
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle - my, -maxVerticalAngle, maxVerticalAngle);
        playerCamera.rotation = targetCameraRotation * Quaternion.AngleAxis(currentHorizontalAngle, Vector3.up) * Quaternion.AngleAxis(currentVerticalAngle, Vector3.right);

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, leverLayer))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj != currentHighlighted) { RemoveHighlight(); currentHighlighted = obj; ApplyHighlight(); }
        }
        else RemoveHighlight();

        if (Input.GetMouseButtonDown(0)) ClickLever();
        if (Input.GetMouseButtonUp(0)) ReleaseLever();
        if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(KeyCode.Escape)) ExitPuzzleMode();
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
        currentHighlighted.GetComponentInParent<PuzzleLevel1>()?.PullCheckLever();
        currentHighlighted.GetComponentInParent<PuzzleLevel2>()?.PullCheckLever();
        currentHighlighted.GetComponentInParent<PuzzleLevel3>()?.PullCheckLever();
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
        isInPuzzleMode = true; canInteract = false;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (playerMovement != null) playerMovement.LockControl();
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        if (moveCamCoroutine != null) StopCoroutine(moveCamCoroutine);
        moveCamCoroutine = StartCoroutine(MoveCamTo(cameraTargetPosition));
    }

    public void ExitPuzzleMode()
    {
        isInPuzzleMode = false; canInteract = false;
        RemoveHighlight(); ReleaseLever();
        currentVerticalAngle = 0f; currentHorizontalAngle = 0f;
        if (playerMovement != null) playerMovement.UnlockControl();
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        if (moveCamCoroutine != null) StopCoroutine(moveCamCoroutine);
        moveCamCoroutine = StartCoroutine(MoveCamBack());
    }

    IEnumerator MoveCamTo(Transform target)
    {
        if (playerCamera == null || target == null) yield break;
        originalCameraPosition = playerCamera.position;
        originalCameraRotation = playerCamera.rotation;
        float elapsed = 0f, duration = 1f / cameraSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; float t = elapsed / duration;
            playerCamera.position = Vector3.Lerp(originalCameraPosition, target.position, t);
            playerCamera.rotation = Quaternion.Slerp(originalCameraRotation, target.rotation, t);
            yield return null;
        }
        playerCamera.position = target.position; playerCamera.rotation = target.rotation;
        targetCameraRotation = target.rotation; currentVerticalAngle = 0f; currentHorizontalAngle = 0f;
        canInteract = true;
    }

    IEnumerator MoveCamBack()
    {
        if (playerCamera == null) yield break;
        Vector3 sp = playerCamera.position; Quaternion sr = playerCamera.rotation;
        float elapsed = 0f, duration = 1f / cameraSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; float t = elapsed / duration;
            playerCamera.position = Vector3.Lerp(sp, originalCameraPosition, t);
            playerCamera.rotation = Quaternion.Slerp(sr, originalCameraRotation, t);
            yield return null;
        }
        playerCamera.position = originalCameraPosition; playerCamera.rotation = originalCameraRotation;
    }

    void OnGUI()
    {
        if (!isInPuzzleMode || !showCursor || !canInteract) return;
        GUI.DrawTexture(new Rect((Screen.width - cursorSize) * 0.5f, (Screen.height - cursorSize) * 0.5f, cursorSize, cursorSize), cursorTexture);
    }

    void OnDisable() { if (isInPuzzleMode) ExitPuzzleMode(); }
}