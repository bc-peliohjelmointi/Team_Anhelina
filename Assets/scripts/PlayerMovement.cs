using UnityEngine;
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
    public AudioSource walkFootstepSource;   // AudioSource ưalk

    public AudioSource runFootstepSource;    // AudioSource Run

    public float walkPitch = 1.5f;           // 1.5x 
    public float runPitch = 1f;            // 1.5x t


    private CharacterController controller;

    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isGrounded;
    private float currentRunEnergy;
    private bool isOverheated = false; // <--- close when u used all sprint

    private float jumpCooldown = 0.5f;
    private float lastJumpTime = -999f;

    //смерть от падения
    [Header("Death")]
    public GameObject deathCanvas;
    public CreditsSlideshow slideshow;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animController = GetComponentInChildren<PlayerAnimationController>();
        Cursor.lockState = CursorLockMode.Locked;
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
    void Die()
    {
        isDead = true;
        enabled = false;

        // Включаем Canvas
        deathCanvas.SetActive(true);

        // Запускаем слайдшоу
        slideshow.StartSlideshow();
    }

    void Update()

    {
        // === check when it touch ground ===
        isGrounded = controller.isGrounded ||
                     Physics.Raycast(transform.position, Vector3.down, controller.height / 2f + 0.25f);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");


        bool shift = Input.GetKey(KeyCode.LeftShift);
        animController.SetMovement(x, z, shift);




        // Shift để chạy nhanh
        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;


        bool wantsToRun = Input.GetKey(KeyCode.LeftShift);
        float speed = walkSpeed;

        // === run ===
        if (!isOverheated && wantsToRun && isMoving && currentRunEnergy > 0f)
        {
            speed = runSpeed;
            currentRunEnergy -= energyDrainRate * Time.deltaTime;
            if (currentRunEnergy <= 0f)
            {
                currentRunEnergy = 0f;
                isOverheated = true; // close run
            }
        }
        else
        {
            // reload energy
            if (currentRunEnergy < maxRunEnergy)
            {
                currentRunEnergy += energyRegenRate * Time.deltaTime;
                if (currentRunEnergy >= maxRunEnergy)
                {
                    currentRunEnergy = maxRunEnergy;
                    isOverheated = false; // open energy again when full
                }
            }
        }

        // === move ===
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // === Footstep Audio ===
        if (isGrounded && isMoving)
        {
            if (!isOverheated && speed == runSpeed) // chạy
            {
                if (!runFootstepSource.isPlaying)
                    runFootstepSource.Play();
                if (walkFootstepSource.isPlaying)
                    walkFootstepSource.Stop();
            }
            else // đi bộ
            {
                if (!walkFootstepSource.isPlaying)
                    walkFootstepSource.Play();
                if (runFootstepSource.isPlaying)
                    runFootstepSource.Stop();
            }
        }
        else
        {
            if (walkFootstepSource.isPlaying) walkFootstepSource.Stop();
            if (runFootstepSource.isPlaying) runFootstepSource.Stop();
        }

        // === jump ===
        if (isGrounded && Input.GetButtonDown("Jump") && Time.time - lastJumpTime >= jumpCooldown)
        {
            animController.SetJump(true);
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpTime = Time.time;

            
        }

        // === gravity ===
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

        // === Camera ===
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // === UI ===
        if (runEnergyBar != null)
        {
            runEnergyBar.fillAmount = currentRunEnergy / maxRunEnergy;

            if (currentRunEnergy >= maxRunEnergy)
                runEnergyUI.SetActive(false);
            else
                runEnergyUI.SetActive(true);

            runEnergyBar.color = isOverheated ? new Color(1f, 0f, 0f) : new Color(0.7f, 0f, 1f);

        }
        // === Fall death check ===
        if (isDead) return;

        // начало падения
        if (!isGrounded && !isFalling)
        {
            isFalling = true;
            startFallY = transform.position.y;
        }

        // приземление
        if (isGrounded && isFalling)
        {
            float fallDistance = startFallY - transform.position.y;

            if (fallDistance >= deathHeight)
            {
                Die();
            }

            isFalling = false;
        }
    }
}
