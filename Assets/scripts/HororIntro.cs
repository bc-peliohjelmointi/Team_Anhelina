using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class HorrorIntro : MonoBehaviour
{
    [Header("UI")]
    public Image blackScreen;
    public Image whiteFlash;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Player")]
    public PlayerMovement playerMovement;
    public Transform playerCamera;

    [Header("Camera Lift Settings")]
    public float introCameraStartY = 2.11f;
    public float introCameraEndY = 1.6f;
    public float introCameraLiftDuration = 6.7f;

    [Header("Timing")]
    public float startDelay = 2f;
    public float flashBeforeEnd = 1.9f;
    public float blackFadeBeforeAudio = 1f;
    public float fadeDuration = 0.4f;

    [Header("Flash Settings")]
    public int flickerCount = 6;
    public float flashAlpha = 0.6f;
    public float flashSpeed = 0.06f;

    private bool isPlaying = false;
    private bool isCameraLifting = false;

    private Vector3 originalCamPos;

    public event Action OnIntroEnd;

    void Start()
    {
        // Hide cursor immediately
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (blackScreen == null)
            blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();

        if (whiteFlash == null)
            whiteFlash = GameObject.Find("WhiteScreen").GetComponent<Image>();

        if (audioSource == null)
            audioSource = FindObjectOfType<AudioSource>();

        if (playerCamera != null)
        {
            originalCamPos = playerCamera.localPosition;
        }

        // LOCK camera from PlayerMovement immediately (fix jitter)
        if (playerMovement != null)
            playerMovement.LockCamera();

        // Setup intro camera pose
        if (playerCamera != null && WillPlayIntro())
        {
            Vector3 camPos = originalCamPos;
            camPos.y = introCameraStartY;
            playerCamera.localPosition = camPos;

            playerCamera.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        OnIntroEnd += StartCameraLift;

        if (WillPlayIntro())
            StartCoroutine(PlayIntro());
        else
        {
            blackScreen.gameObject.SetActive(false);

            if (playerMovement != null)
            {
                playerMovement.enabled = true;
                playerMovement.UnlockCamera();
            }

            OnIntroEnd?.Invoke();
        }
    }

    bool WillPlayIntro()
    {
        int isNewGame = PlayerPrefs.GetInt("NewGame", 0);
        return isNewGame == 1;
    }

    IEnumerator PlayIntro()
    {
        if (isPlaying) yield break;
        isPlaying = true;

        // Disable movement + lock camera
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.LockCamera();
        }

        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("Missing AudioSource or Clip!");
            yield break;
        }

        blackScreen.gameObject.SetActive(true);
        blackScreen.color = Color.black;

        whiteFlash.gameObject.SetActive(true);
        whiteFlash.color = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(startDelay);

        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();

        bool flashDone = false;
        bool blackFadeStarted = false;

        bool liftStarted = false;
        float liftStartTime = audioSource.clip.length - 1f;

        while (audioSource.isPlaying)
        {
            float elapsed = audioSource.time;
            float remaining = audioSource.clip.length - audioSource.time;

            if (!flashDone && remaining <= flashBeforeEnd)
            {
                flashDone = true;
                StartCoroutine(FlashFlicker());
            }

            if (!blackFadeStarted && remaining <= blackFadeBeforeAudio)
            {
                blackFadeStarted = true;
                blackScreen.CrossFadeAlpha(0f, fadeDuration, false);
            }

            // Start camera lift BEFORE audio ends
            if (!liftStarted && elapsed >= liftStartTime)
            {
                liftStarted = true;
                StartCameraLift();
            }

            yield return null;
        }

        // Enable movement again
        if (playerMovement != null)
            playerMovement.enabled = true;

        isPlaying = false;
    }

    IEnumerator FlashFlicker()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashSpeed;
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(0, flashAlpha, t));
                yield return null;
            }

            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashSpeed;
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(flashAlpha, 0, t));
                yield return null;
            }
        }

        whiteFlash.color = new Color(1, 1, 1, 0);
    }

    void StartCameraLift()
    {
        if (playerCamera != null && !isCameraLifting)
        {
            isCameraLifting = true;
            StartCoroutine(CameraLiftRoutine());
        }
    }

    IEnumerator CameraLiftRoutine()
    {
        if (playerCamera == null) yield break;

        float timer = 0f;

        // Start state: cúi đầu, y = introCameraStartY
        Quaternion startRot = Quaternion.Euler(90f, 0f, 0f);
        Quaternion endRot = Quaternion.Euler(0f, 0f, 0f);

        Vector3 startPos = originalCamPos;
        startPos.y = introCameraStartY;

        Vector3 liftEndPos = originalCamPos;
        liftEndPos.y = introCameraEndY; // kết thúc lift ở đây

        playerCamera.localRotation = startRot;
        playerCamera.localPosition = startPos;

        while (timer < introCameraLiftDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / introCameraLiftDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            playerCamera.localRotation = Quaternion.Slerp(startRot, endRot, t);
            playerCamera.localPosition = Vector3.Lerp(startPos, liftEndPos, t);

            yield return null;
        }

        // Gán chính xác vị trí lift cuối → camera không nhảy xuống
        playerCamera.localRotation = endRot;
        playerCamera.localPosition = liftEndPos;

        isCameraLifting = false;

        if (playerMovement != null)
        {
            // khóa luôn chiều cao camera để không tụt xuống
            playerMovement.freezeCameraHeight = true;
            playerMovement.currentCameraHeight = liftEndPos.y;
            playerMovement.targetCameraHeight = liftEndPos.y;

            // unlock camera control để người chơi vẫn nhìn được
            playerMovement.UnlockCamera();
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isPlaying = false;
    }
}