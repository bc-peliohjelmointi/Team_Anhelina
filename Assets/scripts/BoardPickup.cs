using UnityEngine;

public class BoardPickup : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Transform playerCamera;

    [Header("References")]
    public PSMenuNavigation psMenuNavigation;

    [Header("Board Settings")]
    public GameObject boardObject;
    public Vector3 boardFullyVisiblePosition = new Vector3(0, -0.4f, 0.6f);
    public Vector3 boardHiddenPosition = new Vector3(0, -1.5f, 0.6f);
    public Vector3 boardRotation = new Vector3(30f, 0f, 0f);

    [Header("Board Visibility")]
    public float minAngleToShow = 65f;
    public float maxAngleToShow = 80f;

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
    }

    void Update()
    {
        if (hasPickedUpBoard)
        {
            if (isBoardInHand)
            {
                UpdateBoardVisibility();

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

    void UpdateBoardVisibility()
    {
        if (playerCamera == null || boardObject == null) return;

        float cameraXRotation = playerCamera.localEulerAngles.x;

        if (cameraXRotation > 180f)
        {
            cameraXRotation -= 360f;
        }

        float slideValue = 0f;

        if (cameraXRotation >= minAngleToShow && cameraXRotation <= maxAngleToShow)
        {
            slideValue = Mathf.InverseLerp(minAngleToShow, maxAngleToShow, cameraXRotation);
        }
        else if (cameraXRotation > maxAngleToShow)
        {
            slideValue = 1f;
        }

        Vector3 targetPos = Vector3.Lerp(boardHiddenPosition, boardFullyVisiblePosition, slideValue);
        boardObject.transform.localPosition = targetPos;
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
        boardObject.transform.localPosition = boardHiddenPosition;
        boardObject.transform.localRotation = Quaternion.Euler(boardRotation);
    }

    void PutBoardAway()
    {
        if (boardObject == null) return;

        isBoardInHand = false;

        boardObject.transform.SetParent(originalBoardParent);
        boardObject.transform.position = originalBoardPosition;
        boardObject.transform.rotation = originalBoardRotation;
    }

    public bool HasBoard()
    {
        return hasPickedUpBoard;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}