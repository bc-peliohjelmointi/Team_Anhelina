using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Stair Climbing")]
    public bool enableStairClimbing = true;
    public GameObject[] stairObjects;
    public float maxStepHeight = 0.4f;
    public float forwardCheckDistance = 0.5f;
    public float stepUpForce = 0.3f;

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

    private Vector3 velocity;
    private float xRotation = 0f;
    private float currentRunEnergy;
    private bool isOverheated;
    private bool isDead;
    private bool isFalling;
    private float startFallY;
    private float lastJumpTime;
    private float jumpCooldown = 0.5f;
    private bool isControlLocked;
    private bool isCrouching = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animController = GetComponentInChildren<PlayerAnimationController>();

        currentRunEnergy = maxRunEnergy;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null)
        {
            xRotation = playerCamera.localEulerAngles.x;
            if (xRotation > 180f) xRotation -= 360f;
        }

        controller.height = normalHeight;
        controller.center = new Vector3(0, normalHeight / 2f, 0);
    }

    void Update()
    {
        if (isDead || isControlLocked)
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
        HandleFallDeath();
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
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCameraY, Time.deltaTime * crouchSpeed);
            playerCamera.localPosition = camPos;
        }
    }

    void HandleMovement()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
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

        if (enableStairClimbing && isMoving && isGrounded && !isCrouching)
        {
            ClimbStairs(move.normalized);
        }

        controller.Move(move * speed * Time.deltaTime);

        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime > jumpCooldown && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;

            if (animController != null)
                animController.SetJump(true);
        }

        velocity.y += gravity * fallMultiplier * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void ClimbStairs(Vector3 moveDirection)
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        RaycastHit hit;
        if (Physics.Raycast(rayStart, moveDirection, out hit, forwardCheckDistance))
        {
            if (IsStairObject(hit.collider.gameObject))
            {
                float heightDiff = hit.point.y - transform.position.y;

                if (heightDiff > 0.05f && heightDiff <= maxStepHeight)
                {
                    Vector3 upperCheck = rayStart + Vector3.up * maxStepHeight;

                    if (!Physics.Raycast(upperCheck, moveDirection, forwardCheckDistance))
                    {
                        controller.Move(Vector3.up * stepUpForce);
                    }
                }
            }
        }
    }

    bool IsStairObject(GameObject obj)
    {
        if (stairObjects == null || stairObjects.Length == 0) return false;

        foreach (GameObject stair in stairObjects)
        {
            if (stair == null) continue;

            if (obj == stair || obj.transform.IsChildOf(stair.transform))
            {
                return true;
            }
        }

        return false;
    }

    void HandleMouseLook()
    {
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

    void HandleFallDeath()
    {
        if (!controller.isGrounded && !isFalling)
        {
            isFalling = true;
            startFallY = transform.position.y;
        }

        if (controller.isGrounded && isFalling)
        {
            float fallDistance = startFallY - transform.position.y;

            if (fallDistance >= deathHeight)
                Die();

            isFalling = false;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead) return;

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

    void Die()
    {
        if (isDead) return;

        isDead = true;
        velocity = Vector3.zero;
        controller.enabled = false;

        if (walkFootstepSource) walkFootstepSource.Stop();
        if (runFootstepSource) runFootstepSource.Stop();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        if (deathCanvas != null)
            deathCanvas.SetActive(true);

        if (slideshow != null)
            slideshow.StartSlideshow();
    }
}