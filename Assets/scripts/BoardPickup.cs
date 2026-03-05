using Unity.VisualScripting;
using UnityEngine;

public class BoardPickup : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Transform playerCamera;

    [Header("References")]
    public PSMenuNavigation psMenuNavigation;

    [Header("Board Object")]
    public GameObject boardObject;

    [Header("Visible State (80 degrees)")]
    public Vector3 visiblePosition = new Vector3(0, -0.4f, 0.6f);
    public Vector3 visibleRotation = new Vector3(30f, 0f, 0f);

    [Header("Hidden State (0-65 degrees)")]
    public Vector3 hiddenPosition = new Vector3(0, -1.5f, 0.6f);
    public Vector3 hiddenRotation = new Vector3(30f, 0f, 0f);

    [Header("Camera Angle Settings")]
    public float startShowAngle = 65f;
    public float fullyVisibleAngle = 80f;

    [Header("Animation")]
    public float smoothSpeed = 10f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Interaction")]
    public float interactionDistance = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public Canvas interactionPromptCanvas;
    public GameObject interactionPromptObject;

    private bool isNearBoard = false;
    private bool hasPickedUpBoard = false;
    private bool isBoardInHand = false;
    private Vector3 originalBoardPosition;
    private Quaternion originalBoardRotation;
    private Transform originalBoardParent;
    private Vector3 currentTargetPosition;
    private Quaternion currentTargetRotation;

    void Start()
    {
        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.gameObject.SetActive(false);
        }

        if (interactionPromptObject != null)
        {
            interactionPromptObject.SetActive(false);
        }

        if (boardObject != null)
        {
            originalBoardPosition = boardObject.transform.position;
            originalBoardRotation = boardObject.transform.rotation;
            originalBoardParent = boardObject.transform.parent;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        currentTargetPosition = hiddenPosition;
        currentTargetRotation = Quaternion.Euler(hiddenRotation);
    }

    void Update()
    {
        if (hasPickedUpBoard)
        {
            if (isBoardInHand)
            {
                UpdateBoardTransform();

                if (Input.GetKeyDown(pickupKey))
                {
                    PutBoardAway();
                }
            }
            else
            {
                if (Input.GetKeyDown(pickupKey))
                {
                    TakeBoardInHand();
                }
            }
            return;
        }

        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        isNearBoard = distance <= interactionDistance;

        bool canPickup = isNearBoard && AreAllEpisodesComplete();

        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.gameObject.SetActive(canPickup);
        }

        if (interactionPromptObject != null)
        {
            interactionPromptObject.SetActive(canPickup);
        }

        if (canPickup && Input.GetKeyDown(pickupKey))
        {
            PickupBoard();
        }
    }

    void UpdateBoardTransform()
    {
        if (playerCamera == null || boardObject == null) return;

        float cameraXRotation = GetCameraXRotation();
        float slideValue = CalculateSlideValue(cameraXRotation);
        float curvedValue = slideCurve.Evaluate(slideValue);

        currentTargetPosition = Vector3.Lerp(hiddenPosition, visiblePosition, curvedValue);
        currentTargetRotation = Quaternion.Lerp(
            Quaternion.Euler(hiddenRotation),
            Quaternion.Euler(visibleRotation),
            curvedValue
        );

        boardObject.transform.localPosition = Vector3.Lerp(
            boardObject.transform.localPosition,
            currentTargetPosition,
            Time.deltaTime * smoothSpeed
        );

        boardObject.transform.localRotation = Quaternion.Slerp(
            boardObject.transform.localRotation,
            currentTargetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    float GetCameraXRotation()
    {
        float rotation = playerCamera.localEulerAngles.x;

        if (rotation > 180f)
        {
            rotation -= 360f;
        }

        return rotation;
    }

    float CalculateSlideValue(float cameraAngle)
    {
        if (cameraAngle < startShowAngle)
        {
            return 0f;
        }
        else if (cameraAngle >= fullyVisibleAngle)
        {
            return 1f;
        }
        else
        {
            return Mathf.InverseLerp(startShowAngle, fullyVisibleAngle, cameraAngle);
        }
    }

    bool AreAllEpisodesComplete()
    {
        if (psMenuNavigation == null) return false;

        if (psMenuNavigation.checkmarkObjects == null) return false;

        for (int i = 0; i < psMenuNavigation.checkmarkObjects.Length; i++)
        {
            if (psMenuNavigation.checkmarkObjects[i] == null) return false;
            if (!psMenuNavigation.checkmarkObjects[i].activeSelf) return false;
        }

        return true;
    }

    void PickupBoard()
    {
        if (boardObject == null || playerCamera == null) return;

        hasPickedUpBoard = true;

        Rigidbody rb = boardObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Collider col = boardObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.gameObject.SetActive(false);
        }

        if (interactionPromptObject != null)
        {
            interactionPromptObject.SetActive(false);
        }

        TakeBoardInHand();
    }

    void TakeBoardInHand()
    {
        if (boardObject == null || playerCamera == null) return;

        isBoardInHand = true;

        boardObject.transform.SetParent(playerCamera);
        boardObject.transform.localPosition = hiddenPosition;
        boardObject.transform.localRotation = Quaternion.Euler(hiddenRotation);

        currentTargetPosition = hiddenPosition;
        currentTargetRotation = Quaternion.Euler(hiddenRotation);
    }

    void PutBoardAway()
    {
        if (boardObject == null) return;

        isBoardInHand = false;

        boardObject.transform.SetParent(originalBoardParent);
        boardObject.transform.position = originalBoardPosition;
        boardObject.transform.rotation = originalBoardRotation;

        Rigidbody rb = boardObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Collider col = boardObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
    }

    public bool HasBoard()
    {
        return hasPickedUpBoard;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 hiddenPos = playerCamera.TransformPoint(hiddenPosition);
            Gizmos.DrawWireCube(hiddenPos, Vector3.one * 0.1f);

            Gizmos.color = Color.cyan;
            Vector3 visiblePos = playerCamera.TransformPoint(visiblePosition);
            Gizmos.DrawWireCube(visiblePos, Vector3.one * 0.1f);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(hiddenPos, visiblePos);
        }
    }
}
