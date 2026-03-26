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
    public MonoBehaviour playerMovement; // assign your movement script here
    public Transform playerCamera;       // assign your camera transform here

    [Header("Camera Lift Settings")]
    public float introCameraStartY = 2.11f;   // camera low position at intro start (head down)
    public float introCameraEndY = 1.6f;     // normal camera height
    public float introCameraLiftDuration = 6.7f; // duration of camera lifting

    [Header("Timing")]
    public float startDelay = 2f;             // delay before audio starts
    public float flashBeforeEnd = 1.5f;       // when flash starts before audio ends
    public float blackFadeBeforeAudio = 1f;   // when black screen starts fading
    public float fadeDuration = 0.4f;         // fade duration

    [Header("Flash Settings")]
    public int flickerCount = 6;     // number of flickers
    public float flashAlpha = 0.6f;  // brightness of flash
    public float flashSpeed = 0.06f; // speed of each flicker

    private bool isPlaying = false;
    private bool isCameraLifting = false;

    // Event fired when intro finishes
    public event Action OnIntroEnd;

    void Start()
    {
        // Auto find references if not assigned
        if (blackScreen == null)
            blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();

        if (whiteFlash == null)
            whiteFlash = GameObject.Find("WhiteScreen").GetComponent<Image>();

        if (audioSource == null)
            audioSource = FindObjectOfType<AudioSource>();

        // Initialize camera position
        if (playerCamera != null && WillPlayIntro())
        {
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = introCameraStartY;
            playerCamera.localPosition = camPos;
        }

        // Subscribe camera lift to intro end
        OnIntroEnd += StartCameraLift;

        // Check PlayerPrefs for New Game flag
        if (WillPlayIntro())
            StartCoroutine(PlayIntro());
        else
        {
            // Skip intro → show game immediately
            blackScreen.gameObject.SetActive(false);
            if (playerMovement != null)
                playerMovement.enabled = true;

            // Fire event so camera or other scripts can respond
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

        // Disable player movement during intro
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("Missing AudioSource or Clip!");
            yield break;
        }

        // Activate black screen
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = Color.black;

        // Initialize white flash (hidden at start)
        whiteFlash.gameObject.SetActive(true);
        whiteFlash.color = new Color(1, 1, 1, 0);

        // Initial delay before audio
        yield return new WaitForSeconds(startDelay);

        // Reset and play audio
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();

        bool flashDone = false;
        bool blackFadeStarted = false;

        bool liftStarted = false;
        float liftStartTime = audioSource.clip.length - 1f;

        // Loop while audio is playing
        while (audioSource.isPlaying)
        {
            float elapsed = audioSource.time;
            float remaining = audioSource.clip.length - audioSource.time;

            // Trigger flicker flash near the end
            if (!flashDone && remaining <= flashBeforeEnd)
            {
                flashDone = true;
                StartCoroutine(FlashFlicker());
            }

            // Start fading black screen before audio ends
            if (!blackFadeStarted && remaining <= blackFadeBeforeAudio)
            {
                blackFadeStarted = true;
                blackScreen.CrossFadeAlpha(0f, fadeDuration, false);
            }

            // === START CAMERA LIFT 0.5s BEFORE AUDIO END ===
            if (!liftStarted && elapsed >= liftStartTime)
            {
                liftStarted = true;
                StartCameraLift();
            }
            // === END CAMERA LIFT ===

            yield return null;
        }

        // Re-enable player movement after intro
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Fire event to notify other scripts (camera lift)
        OnIntroEnd?.Invoke();

        isPlaying = false;
    }

    IEnumerator FlashFlicker()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            // Fade in flash
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashSpeed;
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(0, flashAlpha, t));
                yield return null;
            }

            // Fade out flash
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashSpeed;
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(flashAlpha, 0, t));
                yield return null;
            }
        }

        // Ensure flash is fully invisible at the end
        whiteFlash.color = new Color(1, 1, 1, 0);
    }

    void StartCameraLift()
    {
        if (playerCamera != null && !isCameraLifting) // ensure only called once
        {
            isCameraLifting = true;
            StartCoroutine(CameraLiftRoutine());
        }
    }

    IEnumerator CameraLiftRoutine()
    {
        if (playerCamera == null) yield break;

        float timer = 0f;

        // Camera starts looking 90 độ xuống (x = 90)
        Quaternion startRot = Quaternion.Euler(90f, 0f, 0f);
        // Camera ends at normal rotation (x = 0)
        Quaternion endRot = Quaternion.Euler(0f, 0f, 0f);

        // Optionally, keep position same as player
        Vector3 startPos = playerCamera.localPosition;
        Vector3 endPos = startPos; // no vertical move, just rotation
        startPos.y = 1.4f;

        while (timer < introCameraLiftDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / introCameraLiftDuration);
            t = Mathf.SmoothStep(0f, 1f, t); // smooth easing

            // Interpolate rotation
            playerCamera.localRotation = Quaternion.Slerp(startRot, endRot, t);
            playerCamera.localPosition = Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        playerCamera.localRotation = endRot;
        playerCamera.localPosition = endPos;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isPlaying = false;
    }
}