using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerAnimationController animController; // player animator
    private CharacterController controller; // character controller

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.4f;
    public float gravity = -35f;
    public float fallMultiplier = 2.2f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("Stair Climbing")]
    public bool enableStairClimbing = true;
    public float maxStepHeight = 0.4f;
    public float stepCheckDistance = 0.5f;
    public float stepSmoothness = 0.1f;

    [Header("Camera Smoothing")]
    public float cameraVerticalSmooth = 8f;

    [Header("Run Energy")]
    public float maxRunEnergy = 5f;
    public float energyDrainRate = 1f;
    public float energyRegenRate = 0.5f;

    [Header("UI")]
    public Image runEnergyBar;
    public GameObject runEnergyUI;

    [Header("Audio")]
    public AudioSource walkFootstepSource;
    public AudioSource runFootstepSource;

    [Header("Push")]
    public float pushPower = 3f;

    [Header("Death")]
    public float deathHeight = 10f;
    public GameObject deathCanvas;
    public CreditsSlideshow slideshow;

    private Vector3 velocity; // vertical velocity
    private float xRotation = 0f; // camera X rotation
    private float currentRunEnergy;
    private bool isOverheated;
    private bool isDead;
    private bool isFalling;
    private float startFallY;
    private float lastJumpTime;
    private float jumpCooldown = 0.5f;
    private bool isControlLocked;
    [HideInInspector]
    public float currentCameraHeight;

    [HideInInspector]
    public float targetCameraHeight;
    private bool disableCameraControl = false;


    [HideInInspector]
    public bool freezeCameraHeight = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animController = GetComponentInChildren<PlayerAnimationController>();

        currentRunEnergy = maxRunEnergy;  // full stamina

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
        {
            currentCameraHeight = playerCamera.localPosition.y;
            targetCameraHeight = currentCameraHeight;
        }
    }

    void Update()
    {
        if (isDead || isControlLocked)
        {
            if (animController != null)
                animController.SetMovement(0f, 0f, false);  // stop animations

            return;
        }
        if (Time.timeScale == 0f) return;

        if (!disableCameraControl)
        {
            HandleMouseLook(); // rotate camera
            SmoothCameraHeight(); // smooth camera Y
        }
        HandleMovement(); // move player

        UpdateEnergyUI(); // stamina bar
        HandleFallDeath(); // check fall death
    }

    public void LockControl()
    {
        isControlLocked = true;

        velocity = Vector3.zero;

        if (animController != null)
        {
            animController.SetMovement(0f, 0f, false);
            animController.SetJump(false);
        }
    }

    public void UnlockControl()
    {
        isControlLocked = false;
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;  // small downward force to stick

            if (animController != null)
                animController.SetJump(false);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift);

        if (animController != null)
            animController.SetMovement(x, z, wantsToRun); // update animations

        float speed = walkSpeed;

        // handle running + stamina

        if (!isOverheated && wantsToRun && isMoving && currentRunEnergy > 0f)
        {
            speed = runSpeed;
            currentRunEnergy -= energyDrainRate * Time.deltaTime;

            if (currentRunEnergy <= 0f)
            {
                currentRunEnergy = 0f;
                isOverheated = true;
            }
        }
        else
        {
            currentRunEnergy += energyRegenRate * Time.deltaTime;

            if (currentRunEnergy >= maxRunEnergy)
            {
                currentRunEnergy = maxRunEnergy;
                isOverheated = false;
            }
        }

        Vector3 move = transform.right * x + transform.forward * z;

        // stair climbing
        if (enableStairClimbing && isMoving)
        {
            float stepUp = ClimbStairs(move.normalized);
            if (stepUp > 0f)
            {
                targetCameraHeight += stepUp;
            }
        }

        controller.Move(move * speed * Time.deltaTime);

        // jump
        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime > jumpCooldown)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;

            if (animController != null)
                animController.SetJump(true);
        }

        velocity.y += gravity * fallMultiplier * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime); // apply gravity
    }

    float ClimbStairs(Vector3 moveDirection)
    {
        if (moveDirection.magnitude < 0.1f) return 0f;

        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        RaycastHit hitLower;

        // detect step in front
        if (Physics.Raycast(rayOrigin, moveDirection, out hitLower, controller.radius + stepCheckDistance))
        {
            float stepHeight = hitLower.point.y - transform.position.y;

            if (stepHeight > 0.05f && stepHeight <= maxStepHeight)
            {
                Vector3 rayOriginUpper = rayOrigin + Vector3.up * (stepHeight + 0.1f);
                RaycastHit hitUpper;

                if (!Physics.Raycast(rayOriginUpper, moveDirection, out hitUpper, controller.radius + stepCheckDistance))
                {
                    controller.Move(Vector3.up * stepHeight * stepSmoothness);  // climb step
                    return stepHeight * stepSmoothness;
                }
            }
        }

        return 0f;
    }

    void SmoothCameraHeight()
    {
        if (playerCamera == null || freezeCameraHeight) return; 

        currentCameraHeight = Mathf.Lerp(currentCameraHeight, targetCameraHeight, Time.deltaTime * cameraVerticalSmooth);

        Vector3 cameraPos = playerCamera.localPosition;
        cameraPos.y = currentCameraHeight;
        playerCamera.localPosition = cameraPos;

        targetCameraHeight = playerCamera.localPosition.y;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (playerCamera != null)
        {
            Quaternion targetRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerCamera.localRotation = targetRotation;
        }

        transform.Rotate(Vector3.up * mouseX);  // rotate player
    }


    // Update the stamina UI (run energy bar)
    void UpdateEnergyUI()
    {
        if (runEnergyBar == null) return;

        // Fill bar based on current stamina
        runEnergyBar.fillAmount = currentRunEnergy / maxRunEnergy;

        // Show UI only if not full
        if (runEnergyUI != null)
            runEnergyUI.SetActive(currentRunEnergy < maxRunEnergy);

        // Change color if overheated
        runEnergyBar.color = isOverheated ? Color.red : new Color(0.7f, 0f, 1f);
    }

    void HandleFallDeath()
    {

        // Start tracking fall when player leaves ground
        if (!controller.isGrounded && !isFalling)
        {
            isFalling = true;
            startFallY = transform.position.y;
        }

        if (controller.isGrounded && isFalling)
        {
            float fallDistance = startFallY - transform.position.y;


            // Kill player if fallen too far
            if (fallDistance >= deathHeight)
                Die();

            isFalling = false;
        }
    }


    // Called automatically when controller hits another collider
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

        // Instant death if colliding with a car
        if (hit.collider.CompareTag("Car"))
        {
            Die();
            return;
        }

        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            rb.AddForce(pushDir.normalized * pushPower, ForceMode.Force);
        }
    }


    // Disable camera rotation for cutscenes, events
    public void LockCamera()
    {
        disableCameraControl = true;
    }

    public void UnlockCamera()
    {
        disableCameraControl = false;
    }

    // Handle player death
    public void Die()
    {
        if (isDead) return; // avoid multiple triggers

        isDead = true;

        velocity = Vector3.zero; // stop all movement
        controller.enabled = false; // disable character controller

        // Stop footstep sounds
        if (walkFootstepSource) walkFootstepSource.Stop();
        if (runFootstepSource) runFootstepSource.Stop();

        // Show cursor so player can interact with UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Freeze game
        Time.timeScale = 0f;

        // Show death UI
        if (deathCanvas != null)
            deathCanvas.SetActive(true);

        // Start credits slideshow if assigned
        if (slideshow != null)
            slideshow.StartSlideshow();
    }
}
