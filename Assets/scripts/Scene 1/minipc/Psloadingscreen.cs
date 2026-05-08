using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// ============================================================
// PSLoadingScreen.cs
// ============================================================
// Loading screen shown before Nu Pogodi mini-game starts
// Shows game title, characters art area, and PLAY button
// Player must click PLAY to start (or it auto-starts after timeout)
//
// VISUAL LAYOUT (all in one Canvas panel):
//   [Title: "WOLF CATCHES EGGS"]
//   [Character art row: Wolf | Robot Rabbit | Rabbit | Chicken]
//   [Subtitle: "Nu Pogodi Arcade"]
//   [PLAY button]
//
// All text is in English as requested
// ============================================================
public class PSLoadingScreen : MonoBehaviour
{
    [Header("Title")]
    public Text titleText;
    public Text subtitleText;

    [Header("Characters (assign Sprite Images in inspector)")]
    // Image showing wolf sprite
    public Image wolfImage;
    // Image showing robot rabbit (black and white)
    public Image robotRabbitImage;
    // Image showing regular rabbit
    public Image regularRabbitImage;
    // Image showing chicken
    public Image chickenImage;

    [Header("Buttons")]
    public Button playButton;
    // optional: show high score or controls hint
    public Text controlsHintText;

    [Header("Animation")]
    // how long to wait before auto-starting if player doesn't click
    public float autoStartDelay = 0f; // 0 = disabled, only manual click
    // fade in duration
    public float fadeInDuration = 0.5f;
    public CanvasGroup canvasGroup;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip titleMusic;
    public AudioClip buttonClickSound;
    public float musicVolume = 0.6f;

    private bool playerClickedPlay = false;
    private Coroutine autoStartCoroutine;

    void Start()
    {
        // set all text content
        if (titleText != null) titleText.text = "WOLF CATCHES EGGS";
        if (subtitleText != null) subtitleText.text = "Nu Pogodi Arcade";
        if (controlsHintText != null)
            controlsHintText.text = "Controls:  A = top-left   Z = bottom-left\n" +
                                    "           D = top-right  X = bottom-right";
        if (playButton != null)
        {
            // set button label
            Text btnText = playButton.GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = "PLAY";
            playButton.onClick.AddListener(OnPlayClicked);
        }
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // called by PSDesktop.OpenGame() coroutine
    // returns when player clicks PLAY (or auto-start fires)
    public IEnumerator PlayLoadingSequence()
    {
        playerClickedPlay = false;
        gameObject.SetActive(true);

        // fade in
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(t / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // play title music if assigned
        if (titleMusic != null)
        {
            audioSource.clip = titleMusic;
            audioSource.volume = musicVolume;
            audioSource.loop = true;
            audioSource.Play();
        }

        // animate characters (simple bounce)
        StartCoroutine(AnimateCharacters());

        // wait for PLAY click (or auto-start)
        if (autoStartDelay > 0f)
            autoStartCoroutine = StartCoroutine(AutoStart());

        yield return new WaitUntil(() => playerClickedPlay);

        // fade out
        if (canvasGroup != null)
        {
            float t = 0f;
            while (t < 0.3f)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(t / 0.3f);
                yield return null;
            }
        }

        audioSource.Stop();
        gameObject.SetActive(false);
    }

    void OnPlayClicked()
    {
        if (buttonClickSound != null) audioSource.PlayOneShot(buttonClickSound);
        if (autoStartCoroutine != null) StopCoroutine(autoStartCoroutine);
        playerClickedPlay = true;
    }

    IEnumerator AutoStart()
    {
        yield return new WaitForSeconds(autoStartDelay);
        playerClickedPlay = true;
    }

    IEnumerator AnimateCharacters()
    {
        // simple bounce animation for character images
        Image[] chars = { wolfImage, robotRabbitImage, regularRabbitImage, chickenImage };
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * 2f;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == null) continue;
                float offset = Mathf.Sin(timer + i * 1.2f) * 5f;
                Vector2 pos = chars[i].rectTransform.anchoredPosition;
                chars[i].rectTransform.anchoredPosition = new Vector2(pos.x, offset);
            }
            yield return null;
        }
    }
}