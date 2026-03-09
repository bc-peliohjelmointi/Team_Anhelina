using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerAnimationController animController;
    private CharacterController controller;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Crouch")]
    public float normalHeight = 1.7f;
    public float crouchHeight = 1.0f;
    public float crouchSpeed = 8f;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.4f;
    public float gravity = -35f;
    public float fallMultiplier = 2.2f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    public float normalCameraHeight = 0.6f;
    public float crouchCameraHeight = 0.2f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobFrequency = 1.5f;
    public float bobWalkAmount = 0.05f;
    public float bobRunAmount = 0.08f;

    [Header("Stair Climbing")]
    public bool enableStairClimbing = true;
    public string stairTag = "Stairs";
    public float stairClimbSpeed = 4f;
    public float stairStepOffset = 0.4f;

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

    private Vector3 velocity;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private float currentRunEnergy;
    private bool isOverheated;
    private bool isControlLocked;
    private bool isCrouching = false;

    private float bobTimer = 0f;
    private Vector3 targetCameraPosition;
    private float lastJumpTime;
    private float jumpCooldown = 0.5f;

    private bool isOnStairs = false;
    private bool cameraInitialized = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animController = GetComponentInChildren<PlayerAnimationController>();

        currentRunEnergy = maxRunEnergy;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        controller.height = normalHeight;
        controller.center = new Vector3(0, normalHeight / 2f, 0);

        if (playerCamera != null)
        {
            targetCameraPosition = playerCamera.localPosition;
            yRotation = transform.eulerAngles.y;
        }
    }

    void Update()
    {
        if (isControlLocked)
        {
            if (animController != null)
                animController.SetMovement(0f, 0f, false);
            return;
        }
        if (Time.timeScale == 0f) return;

        HandleCrouch();
        HandleMovement();
        HandleMouseLook();
        UpdateEnergyUI();

        if (enableHeadBob)
        {
            HandleHeadBob();
        }
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

    void HandleCrouch()
    {
        if (Input.GetKey(crouchKey))
        {
            isCrouching = true;
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            Vector3 checkStart = transform.position + Vector3.up * crouchHeight;
            float checkHeight = normalHeight - crouchHeight + 0.2f;

            if (!Physics.Raycast(checkStart, Vector3.up, checkHeight))
            {
                isCrouching = false;
            }
        }

        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        float targetCameraY = isCrouching ? crouchCameraHeight : normalCameraHeight;

        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchSpeed);
        controller.center = new Vector3(0, controller.height / 2f, 0);

        if (playerCamera != null)
        {
            targetCameraPosition.y = targetCameraY;
        }
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0 && !isOnStairs)
        {
            velocity.y = -2f;

            if (animController != null)
                animController.SetJump(false);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        if (animController != null)
            animController.SetMovement(x, z, wantsToRun);

        float speed = walkSpeed;
        if (isCrouching) speed = walkSpeed * 0.5f;

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

        if (enableStairClimbing && isOnStairs && z > 0.1f)
        {
            Vector3 stairMove = move * speed * Time.deltaTime;
            stairMove.y = stairClimbSpeed * Time.deltaTime;
            controller.Move(stairMove);
        }
        else
        {
            controller.Move(move * speed * Time.deltaTime);
        }

        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime > jumpCooldown && !isCrouching && !isOnStairs)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;

            if (animController != null)
                animController.SetJump(true);
        }

        if (!isOnStairs)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y = 0f;
        }

        controller.Move(velocity * Time.deltaTime);

        isOnStairs = false;
    }

    void HandleHeadBob()
    {
        if (playerCamera == null) return;

        bool isGrounded = controller.isGrounded;
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f) && (isGrounded || isOnStairs);

        if (isMoving)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
            float bobAmount = isRunning ? bobRunAmount : bobWalkAmount;
            float frequency = isRunning ? bobFrequency * 1.5f : bobFrequency;

            bobTimer += Time.deltaTime * frequency;

            float bobOffset = Mathf.Sin(bobTimer) * bobAmount;
            targetCameraPosition.y = (isCrouching ? crouchCameraHeight : normalCameraHeight) + bobOffset;
        }
        else
        {
            bobTimer = 0f;
            targetCameraPosition.y = isCrouching ? crouchCameraHeight : normalCameraHeight;
        }

        playerCamera.localPosition = Vector3.Lerp(
            playerCamera.localPosition,
            targetCameraPosition,
            Time.deltaTime * 10f
        );
    }

    void HandleMouseLook()
    {
        if (!cameraInitialized)
        {
            if (playerCamera != null)
            {
                xRotation = 0f;
                playerCamera.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }
            cameraInitialized = true;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void UpdateEnergyUI()
    {
        if (runEnergyBar == null) return;

        runEnergyBar.fillAmount = currentRunEnergy / maxRunEnergy;

        if (runEnergyUI != null)
            runEnergyUI.SetActive(currentRunEnergy < maxRunEnergy);

        runEnergyBar.color = isOverheated ? Color.red : new Color(0.7f, 0f, 1f);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (enableStairClimbing && hit.collider.CompareTag(stairTag))
        {
            isOnStairs = true;
        }

        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            rb.AddForce(pushDir.normalized * pushPower, ForceMode.Force);
        }
    }
}