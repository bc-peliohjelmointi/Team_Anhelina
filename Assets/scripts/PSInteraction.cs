using UnityEngine;
using System.Collections;

public class PSInteraction : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Transform playerCamera;

    [Header("Camera Target")]
    public Transform cameraTargetPosition;
    public float cameraTransitionSpeed = 2f;

    [Header("Interaction")]
    public float interactionDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    public Canvas interactionPromptCanvas;
    public GameObject interactionPromptObject;

    [Header("Requirements")]
    public PSButton psButton;

    [Header("Menu Navigation")]
    public PSMenuNavigation menuNavigation;

    private bool isNearPS = false;
    private bool isInteracting = false;
    private bool isTransitioning = false;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private PlayerMovement playerMovement;

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

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        isNearPS = distance <= interactionDistance;

        bool canInteract = isNearPS && !isInteracting && !isTransitioning;

        if (psButton != null)
        {
            canInteract = canInteract && psButton.IsOn();
        }

        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.gameObject.SetActive(canInteract);
        }

        if (interactionPromptObject != null)
        {
            interactionPromptObject.SetActive(canInteract);
        }

        if (canInteract && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(EnterPSView());
        }
    }

    IEnumerator EnterPSView()
    {
        isTransitioning = true;

        if (interactionPromptCanvas != null)
        {
            interactionPromptCanvas.gameObject.SetActive(false);
        }

        if (interactionPromptObject != null)
        {
            interactionPromptObject.SetActive(false);
        }

        originalCameraPosition = playerCamera.localPosition;
        originalCameraRotation = playerCamera.localRotation;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Vector3 targetPosition = cameraTargetPosition.position;
        Quaternion targetRotation = cameraTargetPosition.rotation;

        Vector3 startPosition = playerCamera.position;
        Quaternion startRotation = playerCamera.rotation;

        float elapsed = 0f;
        float duration = 1f / cameraTransitionSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            playerCamera.position = Vector3.Lerp(startPosition, targetPosition, t);
            playerCamera.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        playerCamera.position = targetPosition;
        playerCamera.rotation = targetRotation;

        isTransitioning = false;
        isInteracting = true;

        if (menuNavigation != null)
        {
            menuNavigation.EnableNavigation();
        }
    }

    public IEnumerator ExitPSView()
    {
        isInteracting = false;
        isTransitioning = true;

        if (menuNavigation != null)
        {
            menuNavigation.DisableNavigation();
        }

        Vector3 targetPosition = player.position + player.rotation * originalCameraPosition;
        Quaternion targetRotation = player.rotation * originalCameraRotation;

        Vector3 startPosition = playerCamera.position;
        Quaternion startRotation = playerCamera.rotation;

        float elapsed = 0f;
        float duration = 1f / cameraTransitionSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            targetPosition = player.position + player.rotation * originalCameraPosition;
            targetRotation = player.rotation * originalCameraRotation;

            playerCamera.position = Vector3.Lerp(startPosition, targetPosition, t);
            playerCamera.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        playerCamera.localPosition = originalCameraPosition;
        playerCamera.localRotation = originalCameraRotation;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isTransitioning = false;
    }

    public bool IsInteracting()
    {
        return isInteracting;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        if (cameraTargetPosition != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(cameraTargetPosition.position, 0.2f);
            Gizmos.DrawLine(transform.position, cameraTargetPosition.position);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(cameraTargetPosition.position, cameraTargetPosition.forward * 0.5f);
        }
    }
}