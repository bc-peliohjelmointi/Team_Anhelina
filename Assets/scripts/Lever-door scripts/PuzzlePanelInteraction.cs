using UnityEngine;
using System.Collections;

public class PuzzlePanelInteraction : MonoBehaviour
{
    [Header("Camera")]
    public Transform playerCamera;
    public Transform cameraTargetPosition;
    public float cameraSpeed = 3f;

    [Header("Raycast")]
    public float rayDistance = 10f;
    public LayerMask leverLayer = -1;

    [Header("Cursor")]
    public bool showCursor = true;
    public int cursorSize = 8;
    public Color cursorColor = Color.white;

    [Header("Puzzle Level")]
    public PuzzleLevel puzzleLevel;

    [Header("Input")]
    public KeyCode exitKey = KeyCode.E;

    private bool isInPuzzleMode = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Texture2D cursorTexture;
    private GameObject currentHighlighted;

    void Start()
    {
        cursorTexture = new Texture2D(1, 1);
        cursorTexture.SetPixel(0, 0, cursorColor);
        cursorTexture.Apply();
    }

    void Update()
    {
        if (isInPuzzleMode)
        {
            HandlePuzzleMode();
        }
    }

    void HandlePuzzleMode()
    {
        CheckLeverHighlight();

        if (Input.GetMouseButtonDown(0))
        {
            ClickLever();
        }

        if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPuzzleMode();
        }
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
            if (puzzleLevel != null)
            {
                puzzleLevel.OnLeverChanged();
            }
        }

        DoubleLever doubleLever = currentHighlighted.GetComponent<DoubleLever>();
        if (doubleLever != null)
        {
            doubleLever.Toggle();
            if (puzzleLevel != null)
            {
                puzzleLevel.OnLeverChanged();
            }
        }

        CheckLeverInteraction checkLever = currentHighlighted.GetComponent<CheckLeverInteraction>();
        if (checkLever != null)
        {
            checkLever.Pull();
        }
    }

    public void EnterPuzzleMode()
    {
        isInPuzzleMode = true;

        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.position;
            originalCameraRotation = playerCamera.rotation;
            StartCoroutine(MoveCameraToTarget());
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.LockControl();
        }
    }

    public void ExitPuzzleMode()
    {
        isInPuzzleMode = false;

        RemoveHighlight();

        if (playerCamera != null)
        {
            StartCoroutine(MoveCameraBack());
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
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
        if (isInPuzzleMode && showCursor)
        {
            float x = (Screen.width - cursorSize) * 0.5f;
            float y = (Screen.height - cursorSize) * 0.5f;
            GUI.DrawTexture(new Rect(x, y, cursorSize, cursorSize), cursorTexture);
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