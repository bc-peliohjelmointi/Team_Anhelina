using UnityEngine;
using System.Collections;

public class PSInteraction : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public Transform playerCamera;

    [Header("Camera Path")]
    public Transform[] cameraPathPoints;
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
    private PlayerMovement playerMovement;
    private Vector3 startCameraWorldPos;
    private Quaternion startCameraWorldRot;

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

        startCameraWorldPos = playerCamera.position;
        startCameraWorldRot = playerCamera.rotation;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (cameraPathPoints != null && cameraPathPoints.Length > 0)
        {
            for (int i = 0; i < cameraPathPoints.Length; i++)
            {
                if (cameraPathPoints[i] == null) continue;

                Vector3 targetPos = cameraPathPoints[i].position;
                Quaternion targetRot = cameraPathPoints[i].rotation;

                Vector3 startPos = playerCamera.position;
                Quaternion startRot = playerCamera.rotation;

                float elapsed = 0f;
                float duration = 1f / cameraTransitionSpeed;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                    playerCamera.position = Vector3.Lerp(startPos, targetPos, t);
                    playerCamera.rotation = Quaternion.Slerp(startRot, targetRot, t);

                    yield return null;
                }
            }
        }

        if (cameraTargetPosition != null)
        {
            Vector3 targetPos = cameraTargetPosition.position;
            Quaternion targetRot = cameraTargetPosition.rotation;

            Vector3 startPos = playerCamera.position;
            Quaternion startRot = playerCamera.rotation;

            float elapsed = 0f;
            float duration = 1f / cameraTransitionSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                playerCamera.position = Vector3.Lerp(startPos, targetPos, t);
                playerCamera.rotation = Quaternion.Slerp(startRot, targetRot, t);

                yield return null;
            }

            playerCamera.position = targetPos;
            playerCamera.rotation = targetRot;
        }

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

        if (cameraPathPoints != null && cameraPathPoints.Length > 0)
        {
            for (int i = cameraPathPoints.Length - 1; i >= 0; i--)
            {
                if (cameraPathPoints[i] == null) continue;

                Vector3 targetPos = cameraPathPoints[i].position;
                Quaternion targetRot = cameraPathPoints[i].rotation;

                Vector3 startPos = playerCamera.position;
                Quaternion startRot = playerCamera.rotation;

                float elapsed = 0f;
                float duration = 1f / cameraTransitionSpeed;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                    playerCamera.position = Vector3.Lerp(startPos, targetPos, t);
                    playerCamera.rotation = Quaternion.Slerp(startRot, targetRot, t);

                    yield return null;
                }
            }
        }

        Vector3 finalTargetPos = startCameraWorldPos;
        Quaternion finalTargetRot = startCameraWorldRot;

        Vector3 finalStartPos = playerCamera.position;
        Quaternion finalStartRot = playerCamera.rotation;

        float finalElapsed = 0f;
        float finalDuration = 1f / cameraTransitionSpeed;

        while (finalElapsed < finalDuration)
        {
            finalElapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, finalElapsed / finalDuration);

            playerCamera.position = Vector3.Lerp(finalStartPos, finalTargetPos, t);
            playerCamera.rotation = Quaternion.Slerp(finalStartRot, finalTargetRot, t);

            yield return null;
        }

        playerCamera.position = startCameraWorldPos;
        playerCamera.rotation = startCameraWorldRot;

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

        if (cameraPathPoints != null && cameraPathPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < cameraPathPoints.Length; i++)
            {
                if (cameraPathPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(cameraPathPoints[i].position, 0.1f);

                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(cameraPathPoints[i].position, cameraPathPoints[i].forward * 0.3f);
                    Gizmos.color = Color.cyan;

                    if (i > 0 && cameraPathPoints[i - 1] != null)
                    {
                        Gizmos.DrawLine(cameraPathPoints[i - 1].position, cameraPathPoints[i].position);
                    }
                }
            }
        }

        if (cameraTargetPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cameraTargetPosition.position, 0.15f);
            Gizmos.DrawRay(cameraTargetPosition.position, cameraTargetPosition.forward * 0.5f);

            if (cameraPathPoints != null && cameraPathPoints.Length > 0 && cameraPathPoints[cameraPathPoints.Length - 1] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(cameraPathPoints[cameraPathPoints.Length - 1].position, cameraTargetPosition.position);
            }
        }
    }
}
