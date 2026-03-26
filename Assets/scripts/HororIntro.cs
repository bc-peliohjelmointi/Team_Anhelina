using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HorrorIntro : MonoBehaviour
{
    [Header("UI")]
    public Image blackScreen;
    public Image whiteFlash;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Player")]
    public MonoBehaviour playerMovement; // assign your movement script here

    [Header("Timing")]
    public float startDelay = 2f;             // delay before audio starts
    public float flashBeforeEnd = 1.5f;       // when flash starts before audio ends
    public float blackFadeBeforeAudio = 1f;   // when black screen starts fading
    public float fadeDuration = 0.4f;         // fade duration

    [Header("Flash Settings")]
    public int flickerCount = 6;     // number of flickers
    public float flashAlpha = 0.6f;  // brightness of flash
    public float flashSpeed = 0.06f; // speed of each flicker

    bool isPlaying = false;

    void Start()
    {
        // Auto find references if not assigned
        if (blackScreen == null)
            blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();

        if (whiteFlash == null)
            whiteFlash = GameObject.Find("WhiteScreen").GetComponent<Image>();

        if (audioSource == null)
            audioSource = FindObjectOfType<AudioSource>();

        // Check PlayerPrefs for New Game flag
        int isNewGame = PlayerPrefs.GetInt("NewGame", 0);
        bool showIntro = isNewGame == 1;

        if (showIntro)
            StartCoroutine(PlayIntro());
        else
        {
            // Skip intro → show game immediately
            blackScreen.gameObject.SetActive(false);

            // Make sure player can move
            if (playerMovement != null)
                playerMovement.enabled = true;
        }
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

        // Loop while audio is playing
        while (audioSource.isPlaying)
        {
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

            yield return null;
        }

        // Re-enable player movement after intro
        if (playerMovement != null)
            playerMovement.enabled = true;

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

    void OnDisable()
    {
        StopAllCoroutines();
        isPlaying = false;
    }
}