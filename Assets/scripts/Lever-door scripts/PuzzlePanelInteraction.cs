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
    public PuzzleLevel puzzleLevel1;
    public PuzzleLevel puzzleLevel2;
    public PuzzleLevel puzzleLevel3;

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
    private CheckLeverInteraction currentCheckLever;

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
                CheckLeverInteraction checkLever = hitObject.GetComponent<CheckLeverInteraction>();

                if (lever != null || doubleLever != null || checkLever != null)
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

        CheckLeverInteraction checkLever = currentHighlighted.GetComponent<CheckLeverInteraction>();
        if (checkLever != null)
        {
            checkLever.Highlight(true);
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

        CheckLeverInteraction checkLever = currentHighlighted.GetComponent<CheckLeverInteraction>();
        if (checkLever != null)
        {
            checkLever.Highlight(false);
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
        }

        DoubleLever doubleLever = currentHighlighted.GetComponent<DoubleLever>();
        if (doubleLever != null)
        {
            doubleLever.Toggle();

            if (puzzleLevel1 != null) puzzleLevel1.OnLeverChanged();
            if (puzzleLevel2 != null) puzzleLevel2.OnLeverChanged();
            if (puzzleLevel3 != null) puzzleLevel3.OnLeverChanged();
        }

        CheckLeverInteraction checkLever = currentHighlighted.GetComponent<CheckLeverInteraction>();
        if (checkLever != null)
        {
            currentCheckLever = checkLever;
            checkLever.Pull();
        }
    }

    void ReleaseLever()
    {
        if (currentCheckLever != null)
        {
            currentCheckLever.Release();
            currentCheckLever = null;
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

        if (currentCheckLever != null)
        {
            currentCheckLever.Release();
            currentCheckLever = null;
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