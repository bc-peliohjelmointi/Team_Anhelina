using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class HorrorIntro : MonoBehaviour
{
    [Header("UI")]
    public Image blackScreen;   // black overlay
    public Image whiteFlash;    // flash effect

    [Header("Audio")]
    public AudioSource audioSource; // intro sound

    [Header("Player")]
    public PlayerMovement playerMovement; // player control
    public Transform playerCamera;        // camera transform

    [Header("Camera Lift Settings")]
    public float introCameraStartY = 2.11f; // start height
    public float introCameraEndY = 1.6f;    // end height
    public float introCameraLiftDuration = 6.7f; // lift time

    [Header("Timing")]
    public float startDelay = 2f;            // delay before audio
    public float flashBeforeEnd = 2.5f;      // when flash starts
    public float blackFadeBeforeAudio = 1f;  // when black fades
    public float fadeDuration = 0.4f;        // fade speed

    [Header("Flash Settings")]
    public int flickerCount = 6;     // number of flashes
    public float flashAlpha = 0.6f;  // flash strength
    public float flashSpeed = 0.06f; // flash speed

    private bool isPlaying = false;        // prevent replay
    private bool isCameraLifting = false; // avoid double lift

    private Vector3 originalCamPos; // store camera pos

    public event Action OnIntroEnd; // event when intro ends

    void Start()
    {
        // lock + hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // auto find UI if missing
        if (blackScreen == null)
            blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();

        if (whiteFlash == null)
            whiteFlash = GameObject.Find("WhiteScreen").GetComponent<Image>();

        // auto find audio
        if (audioSource == null)
            audioSource = FindObjectOfType<AudioSource>();

        // save original camera position
        if (playerCamera != null)
        {
            originalCamPos = playerCamera.localPosition;
        }

        // lock camera early (fix jitter)
        if (playerMovement != null)
            playerMovement.LockCamera();

        // setup intro camera (look down + higher pos)
        if (playerCamera != null && WillPlayIntro())
        {
            Vector3 camPos = originalCamPos;
            camPos.y = introCameraStartY;
            playerCamera.localPosition = camPos;

            playerCamera.localRotation = Quaternion.Euler(90f, 0f, 0f); // look down
        }

        OnIntroEnd += StartCameraLift; // connect event → lift

        if (WillPlayIntro())
        {
            OnIntroEnd += StartCameraLift; // only connect lift when is new game
            StartCoroutine(PlayIntro());
        }
        else
        {
            blackScreen.gameObject.SetActive(false); // skip intro

            if (playerMovement != null)
            {
                playerMovement.enabled = true;
                playerMovement.UnlockCamera(); // give control back
            }

            
        }
    }

    bool WillPlayIntro()
    {
        int isNewGame = PlayerPrefs.GetInt("NewGame", 0); // check flag
        return isNewGame == 1; // only play if new game
    }

    IEnumerator PlayIntro()
    {
        if (isPlaying) yield break; // prevent duplicate
        isPlaying = true;

        // disable movement + lock cam
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.LockCamera();
        }

        // safety check
        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("Missing AudioSource or Clip!");
            yield break;
        }

        // setup visuals
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = Color.black;

        whiteFlash.gameObject.SetActive(true);
        whiteFlash.color = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(startDelay); // wait before start

        // restart audio clean
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();

        bool flashDone = false;
        bool blackFadeStarted = false;

        bool liftStarted = false;
        float liftStartTime = audioSource.clip.length - 1f; // start lift before end

        while (audioSource.isPlaying)
        {
            float elapsed = audioSource.time; // current time
            float remaining = audioSource.clip.length - audioSource.time; // time left

            // trigger flash near end
            if (!flashDone && remaining <= flashBeforeEnd)
            {
                flashDone = true;
                StartCoroutine(FlashFlicker());
            }

            // fade black out near end
            if (!blackFadeStarted && remaining <= blackFadeBeforeAudio)
            {
                blackFadeStarted = true;
                blackScreen.CrossFadeAlpha(0f, fadeDuration, false);
            }

            // start camera lift before audio ends
            if (!liftStarted && elapsed >= liftStartTime)
            {
                liftStarted = true;
                StartCameraLift();
            }

            yield return null; // next frame
        }

        // re-enable movement
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
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(0, flashAlpha, t)); // fade in
                yield return null;
            }

            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashSpeed;
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(flashAlpha, 0, t)); // fade out
                yield return null;
            }
        }

        whiteFlash.color = new Color(1, 1, 1, 0); // reset
    }

    void StartCameraLift()
    {
        if (playerCamera != null && !isCameraLifting)
        {
            isCameraLifting = true;
            StartCoroutine(CameraLiftRoutine()); // run lift
        }
    }

    IEnumerator CameraLiftRoutine()
    {
        if (playerCamera == null) yield break;

        float timer = 0f;

        // rotation: down → normal
        Quaternion startRot = Quaternion.Euler(90f, 0f, 0f);
        Quaternion endRot = Quaternion.Euler(0f, 0f, 0f);

        // position: high → lower
        Vector3 startPos = originalCamPos;
        startPos.y = introCameraStartY;

        Vector3 liftEndPos = originalCamPos;
        liftEndPos.y = introCameraEndY;

        playerCamera.localRotation = startRot;
        playerCamera.localPosition = startPos;

        while (timer < introCameraLiftDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / introCameraLiftDuration);
            t = Mathf.SmoothStep(0f, 1f, t); // smooth motion

            playerCamera.localRotation = Quaternion.Slerp(startRot, endRot, t); // rotate up
            playerCamera.localPosition = Vector3.Lerp(startPos, liftEndPos, t); // move down

            yield return null;
        }

        // force exact final state
        playerCamera.localRotation = endRot;
        playerCamera.localPosition = liftEndPos;

        isCameraLifting = false;

        if (playerMovement != null)
        {
            // lock final camera height
            playerMovement.freezeCameraHeight = true;
            playerMovement.currentCameraHeight = liftEndPos.y;
            playerMovement.targetCameraHeight = liftEndPos.y;

            playerMovement.UnlockCamera(); // allow look
        }
    }

    void OnDisable()
    {
        StopAllCoroutines(); // stop all running coroutines
        isPlaying = false;
    }
}