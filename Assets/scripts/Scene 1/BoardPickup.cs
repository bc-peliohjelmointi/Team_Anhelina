using UnityEngine;

// this script handles picking up the board after all episodes are done
// board slides into view when you look down, hides when you look straight
public class BoardPickup : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Transform playerCamera;

    [Header("References")]
    public PSMenuNavigation psMenuNavigation; // need this to check if all episodes are complete

    [Header("Board Object")]
    public GameObject boardObject;

    [Header("Visible State (80 degrees)")]
    public Vector3 visiblePosition = new Vector3(0, -0.4f, 0.6f);
    public Vector3 visibleRotation = new Vector3(30f, 0f, 0f);

    [Header("Hidden State (below 65 degrees)")]
    public Vector3 hiddenPosition = new Vector3(0, -1.5f, 0.6f);
    public Vector3 hiddenRotation = new Vector3(30f, 0f, 0f);

    [Header("Camera Angle Settings")]
    public float startShowAngle = 65f;    // board starts appearing at this angle
    public float fullyVisibleAngle = 80f; // fully visible at this angle

    [Header("Animation")]
    public float smoothSpeed = 10f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // makes slide feel nicer

    [Header("Interaction")]
    public float interactionDistance = 2f;
    public KeyCode pickupKey = KeyCode.E;
    public Canvas interactionPromptCanvas;
    public GameObject interactionPromptObject;

    private bool isNearBoard = false;
    private bool hasPickedUpBoard = false; // once picked up, stays picked up
    private bool isBoardInHand = false;
    private Vector3 originalBoardPosition;
    private Quaternion originalBoardRotation;
    private Transform originalBoardParent; // save parent so we can put it back

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

        // save original transform so we can restore it when putting board away
        if (boardObject != null)
        {
            originalBoardPosition = boardObject.transform.position;
            originalBoardRotation = boardObject.transform.rotation;
            originalBoardParent = boardObject.transform.parent;
        }

        // try to find player by tag if not assigned manually
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        // if board is already picked up handle in-hand logic separately
        if (hasPickedUpBoard)
        {
            if (isBoardInHand)
            {
                UpdateBoardTransform(); // slide board based on camera angle

                if (Input.GetKeyDown(pickupKey))
                {
                    PutBoardAway(); // put back to original spot
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

        // only allow pickup if close AND all episodes done
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

    // smoothly moves board between hidden and visible based on where camera is looking
    void UpdateBoardTransform()
    {
        if (playerCamera == null || boardObject == null) return;

        float cameraXRotation = GetCameraXRotation();
        float slideValue = CalculateSlideValue(cameraXRotation);
        float curvedValue = slideCurve.Evaluate(slideValue); // apply curve for nicer feel

        Vector3 targetPosition = Vector3.Lerp(hiddenPosition, visiblePosition, curvedValue);
        Quaternion targetRotation = Quaternion.Lerp(
            Quaternion.Euler(hiddenRotation),
            Quaternion.Euler(visibleRotation),
            curvedValue
        );

        // smooth lerp so it doesnt snap instantly
        boardObject.transform.localPosition = Vector3.Lerp(
            boardObject.transform.localPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );

        boardObject.transform.localRotation = Quaternion.Slerp(
            boardObject.transform.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    float GetCameraXRotation()
    {
        float rotation = playerCamera.localEulerAngles.x;

        // unity returns 0-360, convert to -180 to 180 range
        if (rotation > 180f)
        {
            rotation -= 360f;
        }

        return Mathf.Clamp(rotation, -90f, 90f);
    }

    // returns 0 to 1 depending on how far camera is between startShowAngle and fullyVisibleAngle
    float CalculateSlideValue(float cameraAngle)
    {
        if (cameraAngle < startShowAngle)
        {
            return 0f; // not looking down enough
        }
        else if (cameraAngle >= fullyVisibleAngle)
        {
            return 1f; // fully visible
        }
        else
        {
            return (cameraAngle - startShowAngle) / (fullyVisibleAngle - startShowAngle);
        }
    }

    // checks if all 3 checkmarks are active - that means all episodes were solved
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

        // disable physics so it doesnt fall when parented to camera
        Rigidbody rb = boardObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // disable collider so it doesnt block raycasts
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

    // parents board to camera and sets starting position
    void TakeBoardInHand()
    {
        if (boardObject == null || playerCamera == null) return;

        isBoardInHand = true;

        boardObject.transform.SetParent(playerCamera);
        boardObject.transform.localPosition = hiddenPosition;
        boardObject.transform.localRotation = Quaternion.Euler(hiddenRotation);
    }

    // puts board back where it was in the world
    void PutBoardAway()
    {
        if (boardObject == null) return;

        isBoardInHand = false;

        boardObject.transform.SetParent(originalBoardParent);
        boardObject.transform.position = originalBoardPosition;
        boardObject.transform.rotation = originalBoardRotation;

        // re-enable physics
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

    // gizmos to see interaction range and board positions in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 hiddenPos = playerCamera.TransformPoint(hiddenPosition);
            Gizmos.DrawWireCube(hiddenPos, Vector3.one * 0.1f); // yellow = hidden position

            Gizmos.color = Color.cyan;
            Vector3 visiblePos = playerCamera.TransformPoint(visiblePosition);
            Gizmos.DrawWireCube(visiblePos, Vector3.one * 0.1f); // cyan = visible position

            Gizmos.color = Color.white;
            Gizmos.DrawLine(hiddenPos, visiblePos); // line between them
        }
    }
}