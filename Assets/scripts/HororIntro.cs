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

    [Header("Timing")]
    public float startDelay = 2f;             // thời gian black screen đầu
    public float flashBeforeEnd = 1.5f;       // flash nháy trước khi audio kết thúc
    public float blackFadeBeforeAudio = 1f;   // bắt đầu fade black screen trước audio end
    public float fadeDuration = 0.4f;         // thời gian fade black screen

    [Header("Flash Settings")]
    public int flickerCount = 6;     // số lần nháy
    public float flashAlpha = 0.6f;  // độ sáng flash
    public float flashSpeed = 0.06f; // tốc độ mỗi lần nháy

    bool isPlaying = false;

    void Start()
    {
        // Auto find nếu chưa assign
        if (blackScreen == null)
            blackScreen = GameObject.Find("BlackScreen").GetComponent<Image>();

        if (whiteFlash == null)
            whiteFlash = GameObject.Find("WhiteScreen").GetComponent<Image>();

        if (audioSource == null)
            audioSource = FindObjectOfType<AudioSource>();

        // Kiểm tra PlayerPrefs NewGame
        int isNewGame = PlayerPrefs.GetInt("NewGame", 0);
        bool showIntro = isNewGame == 1;

        if (showIntro)
            StartCoroutine(PlayIntro());
        else
            blackScreen.gameObject.SetActive(false); // skip intro, mở mắt luôn
    }

    IEnumerator PlayIntro()
    {
        if (isPlaying) yield break;
        isPlaying = true;

        if (audioSource == null || audioSource.clip == null)
        {
            Debug.LogError("Missing AudioSource or Clip!");
            yield break;
        }

        // BLACK SCREEN
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = Color.black;

        // WHITE FLASH (ẩn ban đầu)
        whiteFlash.gameObject.SetActive(true);
        whiteFlash.color = new Color(1, 1, 1, 0);

        // DELAY đầu
        yield return new WaitForSeconds(startDelay);

        // PLAY AUDIO
        audioSource.Stop();
        audioSource.time = 0f;
        audioSource.Play();

        bool flashDone = false;
        bool blackFadeStarted = false;

        // LOOP kiểm tra thời gian audio
        while (audioSource.isPlaying)
        {
            float remaining = audioSource.clip.length - audioSource.time;

            // FLASH nháy nhiều lần
            if (!flashDone && remaining <= flashBeforeEnd)
            {
                flashDone = true;
                StartCoroutine(FlashFlicker());
            }

            // BLACK SCREEN FADE SỚM
            if (!blackFadeStarted && remaining <= blackFadeBeforeAudio)
            {
                blackFadeStarted = true;
                blackScreen.CrossFadeAlpha(0f, fadeDuration, false);
            }

            yield return null;
        }

        isPlaying = false;
    }

    IEnumerator FlashFlicker()
    {
        for (int i = 0; i < flickerCount; i++)
        {
            // FADE IN
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashSpeed;
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(0, flashAlpha, t));
                yield return null;
            }

            // FADE OUT
            t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / flashSpeed;
                whiteFlash.color = new Color(1, 1, 1, Mathf.Lerp(flashAlpha, 0, t));
                yield return null;
            }
        }

        // đảm bảo tắt hẳn
        whiteFlash.color = new Color(1, 1, 1, 0);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isPlaying = false;
    }
}