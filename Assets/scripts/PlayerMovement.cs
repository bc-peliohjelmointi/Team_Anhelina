using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerAnimationController animController;

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Jump & Gravity (CS Style)")]
    public float jumpHeight = 1.4f;
    public float gravity = -35f;
    public float fallMultiplier = 2.2f;

    [Header("Camera")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;

    [Header("Run (Neon-style)")]
    public float maxRunEnergy = 5f;
    public float energyDrainRate = 1f;
    public float energyRegenRate = 0.5f;

    [Header("UI Elements")]
    public Image runEnergyBar;
    public GameObject runEnergyUI;

    [Header("Footstep Audio")]
    public AudioSource walkFootstepSource;
    public AudioSource runFootstepSource;
    public float walkPitch = 1.5f;
    public float runPitch = 1f;

    [Header("Push Settings")]
    public float pushPower = 3f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private float currentRunEnergy;
    private bool isOverheated = false;

    private float jumpCooldown = 0.5f;
    private float lastJumpTime = -999f;

    [Header("Death")]
    public float deathHeight = 10f;
    public GameObject deathCanvas;
    public CreditsSlideshow slideshow;

    private bool isDead;
    private bool isFalling;
    private float startFallY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animController = GetComponentInChildren<PlayerAnimationController>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        currentRunEnergy = maxRunEnergy;

        if (walkFootstepSource != null)
        {
            walkFootstepSource.loop = true;
            walkFootstepSource.pitch = walkPitch;
        }

        if (runFootstepSource != null)
        {
            runFootstepSource.loop = true;
            runFootstepSource.pitch = runPitch;
        }
    }

    void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainMenu")
            return;
        if (Time.timeScale == 0f)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (isDead) return;

        isGrounded = controller.isGrounded ||
                     Physics.Raycast(transform.position, Vector3.down,
                     controller.height / 2f + 0.25f);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
        bool wantsToRun = Input.GetKey(KeyCode.LeftShift);

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
            if (currentRunEnergy < maxRunEnergy)
            {
                currentRunEnergy += energyRegenRate * Time.deltaTime;
                if (currentRunEnergy >= maxRunEnergy)
                {
                    currentRunEnergy = maxRunEnergy;
                    isOverheated = false;
                }
            }
        }

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (isGrounded && isMoving)
        {
            if (!isOverheated && speed == runSpeed)
            {
                if (!runFootstepSource.isPlaying) runFootstepSource.Play();
                if (walkFootstepSource.isPlaying) walkFootstepSource.Stop();
            }
            else
            {
                if (!walkFootstepSource.isPlaying) walkFootstepSource.Play();
                if (runFootstepSource.isPlaying) runFootstepSource.Stop();
            }
        }
        else
        {
            if (walkFootstepSource.isPlaying) walkFootstepSource.Stop();
            if (runFootstepSource.isPlaying) runFootstepSource.Stop();
        }

        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime >= jumpCooldown)
        {
            animController.SetJump(true);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -3f;
            animController.SetJump(false);
        }
        else
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        if (runEnergyBar != null)
        {
            runEnergyBar.fillAmount = currentRunEnergy / maxRunEnergy;
            runEnergyUI.SetActive(currentRunEnergy < maxRunEnergy);
            runEnergyBar.color = isOverheated ? Color.red : new Color(0.7f, 0f, 1f);
        }

        if (!isGrounded && !isFalling)
        {
            isFalling = true;
            startFallY = transform.position.y;
        }

        if (isGrounded && isFalling)
        {
            float fallDistance = startFallY - transform.position.y;

            if (fallDistance >= deathHeight)
                Die();

            isFalling = false;
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb != null && !rb.isKinematic)
        {
            DraggableObject draggable = hit.collider.GetComponent<DraggableObject>();

            if (draggable != null && draggable.canBePushed)
            {
                Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
                float pushStrength = pushPower * controller.velocity.magnitude;
                draggable.Push(pushDir.normalized, pushStrength * Time.deltaTime);
            }
            else if (draggable == null)
            {
                Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
                rb.AddForce(pushDir.normalized * pushPower * 0.5f, ForceMode.Force);
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        enabled = false;

        if (deathCanvas != null)
            deathCanvas.SetActive(true);

        if (slideshow != null)
            slideshow.StartSlideshow();
    }
}