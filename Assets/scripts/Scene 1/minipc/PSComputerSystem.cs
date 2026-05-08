using UnityEngine;
using System.Collections;
// ============================================================
// PSComputerSystem.cs
// ============================================================
// FLOW:
// 1. Player clicks power button (PSButton calls PowerOn())
// 2. Desktop quad activates, E prompt appears on screen
// 3. Player presses E -> camera smoothly moves to PC view position
// 4. In PC mode: mouse rotates camera up to 90 degrees each side
//    crosshair dot stays visible so player can click desktop icons
// 5. Player presses E again (or clicks power button) -> camera returns
//
// SETUP:
// - Put this script on the PC/monitor GameObject
// - Create an empty GameObject in front of the monitor screen = pcViewPoint
// - PSButton on the power button calls PSComputerSystem.PowerOn()
// - Desktop quad is a child of the monitor, disabled by default
// - ObjectDragRay on camera handles the LMB raycast for clicking icons
// ============================================================
public class PSComputerSystem : MonoBehaviour
{
    public static PSComputerSystem Instance { get; private set; }

    [Header("Camera")]
    public Transform playerCamera;
    // empty GameObject placed directly in front of monitor at eye level
    public Transform pcViewPoint;
    // how fast camera moves to PC position
    public float cameraMoveSpeed = 3f;
    // max horizontal and vertical rotation while in PC mode
    public float maxHorizontalAngle = 90f;
    public float maxVerticalAngle = 45f;
    // mouse sensitivity while looking around in PC mode
    public float mouseSensitivity = 2f;

    [Header("PC State")]
    // the desktop quad or panel - disabled until PC is on
    public GameObject desktopObject;
    // "E - Use PC" prompt shown on player HUD when PC is on and player is near
    public GameObject usePrompt;
    // how close player needs to be to see the E prompt
    public float interactDistance = 3f;

    [Header("Power")]
    // optional screen glow when PC is on
    public AuraHighlight screenGlow;
    // optional monitor light/LED
    public Light powerLight;
    public Color powerOnColor = Color.cyan;
    public AudioSource audioSource;
    public AudioClip powerOnSound;
    public AudioClip powerOffSound;
    public float soundVolume = 0.7f;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public Transform playerTransform;

    // ---- state ----
    private bool isPoweredOn = false;
    private bool isInPCMode = false;
    private bool canInteract = false;
    private bool isTransitioning = false;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    // rotation accumulated while in PC mode
    private float pcYaw = 0f;
    private float pcPitch = 0f;
    // base rotation at the view point
    private Quaternion baseRotation;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (desktopObject != null) desktopObject.SetActive(false);
        if (usePrompt != null) usePrompt.SetActive(false);
        if (powerLight != null) powerLight.enabled = false;
        if (screenGlow != null) screenGlow.SetGlow(false);
    }

    void Update()
    {
        if (!isPoweredOn) return;
        if (isTransitioning) return;

        if (!isInPCMode)
        {
            CheckProximity();
        }
        else
        {
            HandlePCMode();
        }
    }

    void CheckProximity()
    {
        if (playerTransform == null) return;
        float dist = Vector3.Distance(playerTransform.position, transform.position);
        bool near = dist <= interactDistance;
        if (usePrompt != null) usePrompt.SetActive(near);
        if (near && Input.GetKeyDown(KeyCode.E)) EnterPCMode();
    }

    void HandlePCMode()
    {
        // rotate camera with mouse, clamped to maxAngle each side
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity;
        pcYaw = Mathf.Clamp(pcYaw + mx, -maxHorizontalAngle, maxHorizontalAngle);
        pcPitch = Mathf.Clamp(pcPitch - my, -maxVerticalAngle, maxVerticalAngle);
        playerCamera.rotation = baseRotation
            * Quaternion.AngleAxis(pcYaw, Vector3.up)
            * Quaternion.AngleAxis(pcPitch, Vector3.right);

        // press E to exit PC mode
        if (Input.GetKeyDown(KeyCode.E)) ExitPCMode();
    }

    // called by PSButton when power button is clicked
    public void PowerOn()
    {
        if (isPoweredOn) return;
        isPoweredOn = true;
        if (desktopObject != null) desktopObject.SetActive(true);
        if (screenGlow != null) screenGlow.SetGlow(true);
        if (powerLight != null) { powerLight.enabled = true; powerLight.color = powerOnColor; }
        if (powerOnSound != null && audioSource != null)
            audioSource.PlayOneShot(powerOnSound, soundVolume);
    }

    // called by PSButton or E press to turn off
    public void PowerOff()
    {
        if (!isPoweredOn) return;
        if (isInPCMode) { StartCoroutine(ExitThenPowerOff()); return; }
        isPoweredOn = false;
        if (desktopObject != null) desktopObject.SetActive(false);
        if (screenGlow != null) screenGlow.SetGlow(false);
        if (powerLight != null) powerLight.enabled = false;
        if (usePrompt != null) usePrompt.SetActive(false);
        if (powerOffSound != null && audioSource != null)
            audioSource.PlayOneShot(powerOffSound, soundVolume);
    }

    IEnumerator ExitThenPowerOff()
    {
        yield return StartCoroutine(ReturnCamera());
        PowerOff();
    }

    void EnterPCMode()
    {
        if (isInPCMode || isTransitioning) return;
        isTransitioning = true;
        if (usePrompt != null) usePrompt.SetActive(false);
        if (playerMovement != null) playerMovement.LockController(true);
        originalCamPos = playerCamera.position;
        originalCamRot = playerCamera.rotation;
        pcYaw = 0f;
        pcPitch = 0f;
        StartCoroutine(MoveCamera(
            pcViewPoint.position, pcViewPoint.rotation, true));
    }

    void ExitPCMode()
    {
        if (!isInPCMode || isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(ReturnCamera());
    }

    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot, bool entering)
    {
        float elapsed = 0f;
        float duration = 1f / cameraMoveSpeed;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;
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
        isInPCMode = true;
        isTransitioning = false;
        // save base rotation for mouse look
        baseRotation = targetRot;
        canInteract = true;
    }

    IEnumerator ReturnCamera()
    {
        isInPCMode = false;
        canInteract = false;
        float elapsed = 0f;
        float duration = 1f / cameraMoveSpeed;
        Vector3 startPos = playerCamera.position;
        Quaternion startRot = playerCamera.rotation;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            playerCamera.position = Vector3.Lerp(startPos, originalCamPos, t);
            playerCamera.rotation = Quaternion.Slerp(startRot, originalCamRot, t);
            yield return null;
        }
        playerCamera.position = originalCamPos;
        playerCamera.rotation = originalCamRot;
        if (playerMovement != null) playerMovement.LockController(false);
        isTransitioning = false;
    }

    public bool IsInPCMode() => isInPCMode;
    public bool IsPoweredOn() => isPoweredOn;
}