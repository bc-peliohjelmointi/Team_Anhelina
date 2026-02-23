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

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.4f;
    public float gravity = -35f;
    public float fallMultiplier = 2.2f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

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

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animController = GetComponentInChildren<PlayerAnimationController>();

        currentRunEnergy = maxRunEnergy;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isDead) return;
        if (Time.timeScale == 0f) return;

        HandleMovement();
        HandleMouseLook();
        UpdateEnergyUI();
        HandleFallDeath();
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
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift);

        if (animController != null)
            animController.SetMovement(x, z, wantsToRun);

        float speed = walkSpeed;

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
        controller.Move(move * speed * Time.deltaTime);

        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime > jumpCooldown)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;

            if (animController != null)
                animController.SetJump(true);
        }

        velocity.y += gravity * fallMultiplier * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
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