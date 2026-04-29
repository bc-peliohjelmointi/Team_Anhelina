using UnityEngine;
using System.Collections;
// when player walks up and presses E, camera smoothly moves to viewing position
// player can then look around with mouse and click levers
// press E or Escape to exit and camera returns to normal
// leverLayer should match the layer your levers are on, or use -1 for all
public class PuzzlePanelInteraction : MonoBehaviour
{
    // player transform for distance check
    public Transform player;
    // camera transform
    public Transform playerCamera;
    // empty GameObject placed at the ideal panel viewing angle
    public Transform cameraTargetPosition;
    // how fast camera moves to target position
    public float cameraSpeed = 3f;
    // how close player needs to be to get the enter prompt
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    // the "E - Use panel" prompt
    public GameObject interactionPrompt;
    // how far the raycast reaches inside panel mode
    public float rayDistance = 10f;
    // set to your levers layer or -1 for everything
    public LayerMask leverLayer = -1;
    // show a dot cursor in panel mode
    public bool showCursor = true;
    public int cursorSize = 8;
    public Color cursorColor = Color.white;
    // how sensitive mouse movement is in panel mode
    public float mouseSensitivity = 2f;
    // max camera angles from center while in panel mode
    public float maxVerticalAngle = 30f;
    public float maxHorizontalAngle = 40f;
    // all three puzzle levels so we can call release on all of them
    public PuzzleLevel1 puzzleLevel1;
    public PuzzleLevel2 puzzleLevel2;
    public PuzzleLevel3 puzzleLevel3;
    // key to exit panel mode
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

        // mouse controls camera rotation within clamped angle range
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        currentHorizontalAngle = Mathf.Clamp(currentHorizontalAngle + mx, -maxHorizontalAngle, maxHorizontalAngle);
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle - my, -maxVerticalAngle, maxVerticalAngle);
        playerCamera.rotation = targetCameraRotation
            * Quaternion.AngleAxis(currentHorizontalAngle, Vector3.up)
            * Quaternion.AngleAxis(currentVerticalAngle, Vector3.right);

        // check what lever player is looking at
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
        // fallback in case collider is on parent of the level object
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
        isInPuzzleMode = true;
        canInteract = false;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (playerMovement != null) playerMovement.LockControl();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (moveCamCoroutine != null) StopCoroutine(moveCamCoroutine);
        moveCamCoroutine = StartCoroutine(MoveCamTo(cameraTargetPosition));
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
        moveCamCoroutine = StartCoroutine(MoveCamBack());
    }

    IEnumerator MoveCamTo(Transform target)
    {
        if (playerCamera == null || target == null) yield break;
        originalCameraPosition = playerCamera.position;
        originalCameraRotation = playerCamera.rotation;
        float elapsed = 0f;
        float duration = 1f / cameraSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            playerCamera.position = Vector3.Lerp(originalCameraPosition, target.position, t);
            playerCamera.rotation = Quaternion.Slerp(originalCameraRotation, target.rotation, t);
            yield return null;
        }
        playerCamera.position = target.position;
        playerCamera.rotation = target.rotation;
        targetCameraRotation = target.rotation;
        currentVerticalAngle = 0f;
        currentHorizontalAngle = 0f;
        // only allow clicking after camera finishes moving
        canInteract = true;
    }

    IEnumerator MoveCamBack()
    {
        if (playerCamera == null) yield break;
        Vector3 sp = playerCamera.position;
        Quaternion sr = playerCamera.rotation;
        float elapsed = 0f;
        float duration = 1f / cameraSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            playerCamera.position = Vector3.Lerp(sp, originalCameraPosition, t);
            playerCamera.rotation = Quaternion.Slerp(sr, originalCameraRotation, t);
            yield return null;
        }
        playerCamera.position = originalCameraPosition;
        playerCamera.rotation = originalCameraRotation;
    }

    void OnGUI()
    {
        if (!isInPuzzleMode || !showCursor || !canInteract) return;
        GUI.DrawTexture(new Rect(
            (Screen.width - cursorSize) * 0.5f,
            (Screen.height - cursorSize) * 0.5f,
            cursorSize, cursorSize), cursorTexture);
    }

    void OnDisable() { if (isInPuzzleMode) ExitPuzzleMode(); }
}