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

    [Header("Input")]
    public KeyCode exitKey = KeyCode.E;

    private bool isNearPanel = false;
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
    private PuzzleLevel1 currentCheckLevel1;
    private PuzzleLevel2 currentCheckLevel2;
    private PuzzleLevel3 currentCheckLevel3;

    void Start()
    {
        cursorTexture = new Texture2D(1, 1);
        cursorTexture.SetPixel(0, 0, cursorColor);
        cursorTexture.Apply();

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (player == null) return;

        if (!isInPuzzleMode)
        {
            CheckDistance();
        }
        else
        {
            HandlePuzzleMode();
        }
    }

    void CheckDistance()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        isNearPanel = distance <= interactionDistance;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isNearPanel);
        }

        if (isNearPanel && Input.GetKeyDown(interactKey))
        {
            EnterPuzzleMode();
        }
    }

    void HandlePuzzleMode()
    {
        if (!canInteract) return;

        HandleCameraRotation();
        CheckLeverHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            ClickLever();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ReleaseLever();
        }

        if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPuzzleMode();
        }
    }

    void HandleCameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        currentHorizontalAngle += mouseX;
        currentVerticalAngle -= mouseY;

        currentHorizontalAngle = Mathf.Clamp(currentHorizontalAngle, -maxHorizontalAngle, maxHorizontalAngle);
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, -maxVerticalAngle, maxVerticalAngle);

        Quaternion verticalRotation = Quaternion.AngleAxis(currentVerticalAngle, Vector3.right);
        Quaternion horizontalRotation = Quaternion.AngleAxis(currentHorizontalAngle, Vector3.up);

        playerCamera.rotation = targetCameraRotation * horizontalRotation * verticalRotation;
    }

    void CheckLeverHighlight()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, leverLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject != currentHighlighted)
            {
                RemoveHighlight();

                Lever lever = hitObject.GetComponent<Lever>();
                DoubleLever doubleLever = hitObject.GetComponent<DoubleLever>();

                PuzzleLevel1 checkLevel1 = hitObject.GetComponentInParent<PuzzleLevel1>();
                PuzzleLevel2 checkLevel2 = hitObject.GetComponentInParent<PuzzleLevel2>();
                PuzzleLevel3 checkLevel3 = hitObject.GetComponentInParent<PuzzleLevel3>();

                if (lever != null || doubleLever != null || checkLevel1 != null || checkLevel2 != null || checkLevel3 != null)
                {
                    currentHighlighted = hitObject;
                    ApplyHighlight();
                }
            }
        }
        else
        {
            RemoveHighlight();
        }
    }

    void ApplyHighlight()
    {
        if (currentHighlighted == null) return;

        Lever lever = currentHighlighted.GetComponent<Lever>();
        if (lever != null)
        {
            lever.Highlight(true);
        }

        DoubleLever doubleLever = currentHighlighted.GetComponent<DoubleLever>();
        if (doubleLever != null)
        {
            doubleLever.Highlight(true);
        }

        PuzzleLevel1 checkLevel1 = currentHighlighted.GetComponentInParent<PuzzleLevel1>();
        if (checkLevel1 != null)
        {
            checkLevel1.HighlightCheckLever(true);
        }

        PuzzleLevel2 checkLevel2 = currentHighlighted.GetComponentInParent<PuzzleLevel2>();
        if (checkLevel2 != null)
        {
            checkLevel2.HighlightCheckLever(true);
        }

        PuzzleLevel3 checkLevel3 = currentHighlighted.GetComponentInParent<PuzzleLevel3>();
        if (checkLevel3 != null)
        {
            checkLevel3.HighlightCheckLever(true);
        }
    }

    void RemoveHighlight()
    {
        if (currentHighlighted == null) return;

        Lever lever = currentHighlighted.GetComponent<Lever>();
        if (lever != null)
        {
            lever.Highlight(false);
        }

        DoubleLever doubleLever = currentHighlighted.GetComponent<DoubleLever>();
        if (doubleLever != null)
        {
            doubleLever.Highlight(false);
        }

        PuzzleLevel1 checkLevel1 = currentHighlighted.GetComponentInParent<PuzzleLevel1>();
        if (checkLevel1 != null)
        {
            checkLevel1.HighlightCheckLever(false);
        }

        PuzzleLevel2 checkLevel2 = currentHighlighted.GetComponentInParent<PuzzleLevel2>();
        if (checkLevel2 != null)
        {
            checkLevel2.HighlightCheckLever(false);
        }

        PuzzleLevel3 checkLevel3 = currentHighlighted.GetComponentInParent<PuzzleLevel3>();
        if (checkLevel3 != null)
        {
            checkLevel3.HighlightCheckLever(false);
        }

        currentHighlighted = null;
    }

    void ClickLever()
    {
        if (currentHighlighted == null) return;

        Lever lever = currentHighlighted.GetComponent<Lever>();
        if (lever != null)
        {
            lever.Toggle();

            if (puzzleLevel1 != null) puzzleLevel1.OnLeverChanged();
            if (puzzleLevel2 != null) puzzleLevel2.OnLeverChanged();
            if (puzzleLevel3 != null) puzzleLevel3.OnLeverChanged();
            return;
        }

        DoubleLever doubleLever = currentHighlighted.GetComponent<DoubleLever>();
        if (doubleLever != null)
        {
            doubleLever.Toggle();

            if (puzzleLevel1 != null) puzzleLevel1.OnLeverChanged();
            if (puzzleLevel2 != null) puzzleLevel2.OnLeverChanged();
            if (puzzleLevel3 != null) puzzleLevel3.OnLeverChanged();
            return;
        }

        PuzzleLevel1 checkLevel1 = currentHighlighted.GetComponentInParent<PuzzleLevel1>();
        if (checkLevel1 != null)
        {
            currentCheckLevel1 = checkLevel1;
            checkLevel1.PullCheckLever();
            return;
        }

        PuzzleLevel2 checkLevel2 = currentHighlighted.GetComponentInParent<PuzzleLevel2>();
        if (checkLevel2 != null)
        {
            currentCheckLevel2 = checkLevel2;
            checkLevel2.PullCheckLever();
            return;
        }

        PuzzleLevel3 checkLevel3 = currentHighlighted.GetComponentInParent<PuzzleLevel3>();
        if (checkLevel3 != null)
        {
            currentCheckLevel3 = checkLevel3;
            checkLevel3.PullCheckLever();
            return;
        }
    }

    void ReleaseLever()
    {
        if (currentCheckLevel1 != null)
        {
            currentCheckLevel1.ReleaseCheckLever();
            currentCheckLevel1 = null;
        }

        if (currentCheckLevel2 != null)
        {
            currentCheckLevel2.ReleaseCheckLever();
            currentCheckLevel2 = null;
        }

        if (currentCheckLevel3 != null)
        {
            currentCheckLevel3.ReleaseCheckLever();
            currentCheckLevel3 = null;
        }
    }

    public void EnterPuzzleMode()
    {
        isInPuzzleMode = true;
        canInteract = false;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.position;
            originalCameraRotation = playerCamera.rotation;
            StartCoroutine(MoveCameraToTarget());
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovement != null)
        {
            playerMovement.LockControl();
        }
    }

    public void ExitPuzzleMode()
    {
        isInPuzzleMode = false;
        canInteract = false;

        RemoveHighlight();

        if (currentCheckLevel1 != null)
        {
            currentCheckLevel1.ReleaseCheckLever();
            currentCheckLevel1 = null;
        }

        if (currentCheckLevel2 != null)
        {
            currentCheckLevel2.ReleaseCheckLever();
            currentCheckLevel2 = null;
        }

        if (currentCheckLevel3 != null)
        {
            currentCheckLevel3.ReleaseCheckLever();
            currentCheckLevel3 = null;
        }

        currentVerticalAngle = 0f;
        currentHorizontalAngle = 0f;

        if (playerCamera != null)
        {
            StartCoroutine(MoveCameraBack());
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovement != null)
        {
            playerMovement.UnlockControl();
        }
    }

    IEnumerator MoveCameraToTarget()
    {
        if (playerCamera == null || cameraTargetPosition == null) yield break;

        float elapsed = 0f;
        float duration = 1f / cameraSpeed;

        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            playerCamera.position = Vector3.Lerp(startPos, cameraTargetPosition.position, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, cameraTargetPosition.rotation, t);

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

        float elapsed = 0f;
        float duration = 1f / cameraSpeed;

        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;

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
        if (isInPuzzleMode && showCursor && canInteract)
        {
            float x = (Screen.width - cursorSize) * 0.5f;
            float y = (Screen.height - cursorSize) * 0.5f;
            GUI.DrawTexture(new Rect(x, y, cursorSize, cursorSize), cursorTexture);
        }
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
        if (isInPuzzleMode)
        {
            ExitPuzzleMode();
        }
    }
}