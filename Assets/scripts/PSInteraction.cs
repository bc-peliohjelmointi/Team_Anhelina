using UnityEngine;

public class PSInteraction : MonoBehaviour
{
    public Transform player;
    public Transform playerCamera;
    public Transform psViewPosition;
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactionPrompt;
    public PSButton psButton;

    private bool isNearPS = false;
    private bool isInteracting = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        playerMovement = player.GetComponent<PlayerMovement>();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        isNearPS = distance <= interactionDistance;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(isNearPS && !isInteracting);
        }

        if (isNearPS && !isInteracting && Input.GetKeyDown(interactKey))
        {
            if (psButton != null && psButton.IsOn())
            {
                EnterPSView();
            }
        }

        if (isInteracting && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitPSView();
        }
    }

    void EnterPSView()
    {
        isInteracting = true;

        originalCameraPosition = playerCamera.localPosition;
        originalCameraRotation = playerCamera.localRotation;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        playerCamera.position = psViewPosition.position;
        playerCamera.rotation = psViewPosition.rotation;

    }

    public void ExitPSView()
    {
        isInteracting = false;

        playerCamera.localPosition = originalCameraPosition;
        playerCamera.localRotation = originalCameraRotation;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public bool IsInteracting()
    {
        return isInteracting;
    }
}