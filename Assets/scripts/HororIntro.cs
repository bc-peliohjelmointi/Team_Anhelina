using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HorrorIntro : MonoBehaviour
{
    [Header("UI")]
    public Image blackScreen;
    public Image whiteFlash;
    public Image blurScreen;

    [Header("Audio")]
    public AudioSource ambientAudio;

    [Header("Camera Shake")]
    public CameraShake camShake;

    bool isPlaying = false;

    void Start()
    {
        Debug.Log("HORROR INTRO START");

        StartCoroutine(InitAndPlay());
    }

    IEnumerator InitAndPlay()
    {
        yield return null; // đợi scene load xong

        int isNewGame = PlayerPrefs.GetInt("NewGame", 0);
        Debug.Log("NewGame = " + isNewGame);

        if (isNewGame == 1)
        {
            PlayerPrefs.SetInt("NewGame", 0);
            PlayerPrefs.Save();

            yield return StartCoroutine(PlayIntro());
        }
        else
        {
            Debug.Log("SKIP INTRO");

            SkipIntro();
        }
    }

    void SkipIntro()
    {
        if (ambientAudio)
        {
            ambientAudio.Stop();
        }

        if (blackScreen)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0, 0, 0, 0); // KHÔNG ĐỂ ĐEN MÀN HÌNH
        }

        if (whiteFlash)
        {
            whiteFlash.gameObject.SetActive(false);
        }

        if (blurScreen)
        {
            blurScreen.gameObject.SetActive(false);
        }
    }

    IEnumerator PlayIntro()
    {
        if (isPlaying) yield break;
        isPlaying = true;

        if (ambientAudio == null || ambientAudio.clip == null)
        {
            Debug.LogError("Missing Audio");
            yield break;
        }

        // INIT UI
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = Color.black;

        whiteFlash.gameObject.SetActive(true);
        whiteFlash.color = new Color(1, 1, 1, 0);

        if (blurScreen)
        {
            blurScreen.gameObject.SetActive(true);
            blurScreen.color = new Color(0, 0, 0, 0);
        }

        // AUDIO RESET
        ambientAudio.Stop();
        ambientAudio.time = 0f;
        ambientAudio.Play();

        bool flashDone = false;
        bool jumpDone = false;

        while (ambientAudio != null && ambientAudio.isPlaying)
        {
            float remaining = ambientAudio.clip.length - ambientAudio.time;

            // FLASH 2s trước end
            if (!flashDone && remaining <= 2f)
            {
                flashDone = true;
                StartCoroutine(FlashEffect());
            }

            // JUMPSCARE 1s trước end
            if (!jumpDone && remaining <= 1f)
            {
                jumpDone = true;
                StartCoroutine(JumpscareEffect());

                if (camShake)
                    StartCoroutine(camShake.Shake(0.3f, 0.2f));
            }

            yield return null;
        }

        if (blackScreen)
            blackScreen.CrossFadeAlpha(0f, 1f, false);

        isPlaying = false;
    }

    IEnumerator FlashEffect()
    {
        whiteFlash.color = new Color(1, 1, 1, 0.7f);

        yield return new WaitForSecondsRealtime(0.2f);

        if (blurScreen)
            blurScreen.color = new Color(0.6f, 0.45f, 0.25f, 0.5f);

        yield return new WaitForSecondsRealtime(0.5f);

        whiteFlash.color = new Color(1, 1, 1, 0);
    }

    IEnumerator JumpscareEffect()
    {
        whiteFlash.color = Color.white;

        yield return new WaitForSecondsRealtime(0.1f);

        whiteFlash.color = new Color(1, 1, 1, 0);

        if (blurScreen)
            blurScreen.color = new Color(0, 0, 0, 0.4f);
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isPlaying = false;
    }
}