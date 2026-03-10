using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private PlayerAnimationController animController;

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

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float cameraNearClip = 0.01f;

    [Header("Head Bob")]
    public bool enableHeadBob = true;
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;
    public float bobRunMultiplier = 1.5f;

    [Header("Stair Climbing")]
    public bool enableStairClimbing = true;
    public float stairClimbSpeed = 4f;
    public float stairCheckRadius = 0.5f;
    public float stairCheckDistance = 1f;

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

    private Vector3 velocity = Vector3.zero;
    private float verticalRotation = 0f;
    private float currentRunEnergy;
    private bool isOverheated = false;
    private bool isCrouching = false;
    private bool isControlLocked = false;

    private float bobTimer = 0f;
    private Vector3 baseCameraPosition;
    private Vector3 initialCameraLocalPosition;
    private float lastJumpTime = 0f;
    private float jumpCooldown = 0.5f;

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
            initialCameraLocalPosition = playerCamera.localPosition;
            baseCameraPosition = initialCameraLocalPosition;

            verticalRotation = playerCamera.localEulerAngles.x;
            if (verticalRotation > 180f)
                verticalRotation -= 360f;

            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam != null)
            {
                cam.nearClipPlane = cameraNearClip;
            }
        }
    }

    void Update()
    {
        if (isControlLocked || Time.timeScale == 90f)
        {
            if (animController != null)
                animController.SetMovement(0f, 0f, false);
            return;
        }

        HandleCrouch();
        HandleMovement();
        HandleMouseLook();
        HandleHeadBob();
        UpdateEnergyUI();
    }

    void HandleCrouch()
    {
        if (Input.GetKey(crouchKey))
        {
            isCrouching = true;
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            Vector3 checkPos = transform.position + Vector3.up * crouchHeight;
            if (!Physics.Raycast(checkPos, Vector3.up, normalHeight - crouchHeight + 0.2f))
            {
                isCrouching = false;
            }
        }

        float targetHeight = isCrouching ? crouchHeight : normalHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchSpeed);
        controller.center = new Vector3(0, controller.height / 2f, 0);

        float targetCameraY = isCrouching ? initialCameraLocalPosition.y - 0.4f : initialCameraLocalPosition.y;
        baseCameraPosition.y = Mathf.Lerp(baseCameraPosition.y, targetCameraY, Time.deltaTime * crouchSpeed);
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;
        bool isOnStairs = CheckIfOnStairs();

        if (isGrounded && velocity.y < 0 && !isOnStairs)
        {
            velocity.y = -2f;
            if (animController != null)
                animController.SetJump(false);
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && !isCrouching;

        if (animController != null)
            animController.SetMovement(horizontal, vertical, wantsToRun);

        float speed = isCrouching ? walkSpeed * 0.5f : walkSpeed;

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

        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;

        if (enableStairClimbing && isOnStairs && isMoving)
        {
            float verticalMove = 0f;

            if (vertical > 0.1f)
            {
                verticalMove = stairClimbSpeed * Time.deltaTime;
            }
            else if (vertical < -0.1f)
            {
                verticalMove = -stairClimbSpeed * Time.deltaTime;
            }

            Vector3 stairMove = moveDirection * speed * Time.deltaTime;
            stairMove.y = verticalMove;
            controller.Move(stairMove);

            velocity.y = 0f;
        }
        else
        {
            controller.Move(moveDirection * speed * Time.deltaTime);
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
            controller.Move(velocity * Time.deltaTime);
        }
    }

    bool CheckIfOnStairs()
    {
        Vector3 checkPosition = transform.position;

        RaycastHit hitDown;
        if (Physics.Raycast(checkPosition + Vector3.up * 0.1f, Vector3.down, out hitDown, 0.5f))
        {
            if (hitDown.collider.CompareTag("Stairs"))
            {
                return true;
            }
        }

        Vector3 forwardCheck = checkPosition + transform.forward * stairCheckRadius;
        if (Physics.Raycast(forwardCheck + Vector3.up * 0.1f, Vector3.down, out hitDown, 0.5f))
        {
            if (hitDown.collider.CompareTag("Stairs"))
            {
                return true;
            }
        }

        RaycastHit hitForward;
        if (Physics.Raycast(checkPosition + Vector3.up * 0.1f, transform.forward, out hitForward, stairCheckDistance))
        {
            if (hitForward.collider.CompareTag("Stairs"))
            {
                return true;
            }
        }

        Collider[] nearbyColliders = Physics.OverlapSphere(checkPosition, stairCheckRadius);
        foreach (Collider col in nearbyColliders)
        {
            if (col.CompareTag("Stairs"))
            {
                return true;
            }
        }

        return false;
    }

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleHeadBob()
    {
        if (!enableHeadBob || playerCamera == null) return;

        bool isGrounded = controller.isGrounded;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f) && isGrounded;

        if (isMoving)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
            float speedMultiplier = isRunning ? bobRunMultiplier : 1f;

            bobTimer += Time.deltaTime * bobSpeed * speedMultiplier;
            float bobOffsetY = Mathf.Sin(bobTimer) * bobAmount;

            Vector3 targetPosition = baseCameraPosition;
            targetPosition.y += bobOffsetY;
            playerCamera.localPosition = targetPosition;
        }
        else
        {
            bobTimer = 0f;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, baseCameraPosition, Time.deltaTime * 10f);
        }
    }

    void UpdateEnergyUI()
    {
        if (runEnergyBar != null)
        {
            runEnergyBar.fillAmount = currentRunEnergy / maxRunEnergy;
            runEnergyBar.color = isOverheated ? Color.red : new Color(0.7f, 0f, 1f);
        }

        if (runEnergyUI != null)
            runEnergyUI.SetActive(currentRunEnergy < maxRunEnergy);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            Vector3 pushDirection = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            rb.AddForce(pushDirection.normalized * pushPower, ForceMode.Force);
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

    void OnDrawGizmosSelected()
    {
        if (!enableStairClimbing) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stairCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, transform.forward * stairCheckDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 0.5f);
    }
}