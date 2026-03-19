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
    public AudioSource voice1;
    public AudioSource voice2;

    [Header("Camera")]
    public CameraShake camShake;

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        // 🖤 init UI
        blackScreen.gameObject.SetActive(true);
        blackScreen.color = new Color(0, 0, 0, 1);

        whiteFlash.gameObject.SetActive(true);
        whiteFlash.color = new Color(1, 1, 1, 0);

        if (blurScreen != null)
        {
            blurScreen.gameObject.SetActive(true);
            blurScreen.color = new Color(0, 0, 0, 0);
        }

        // 🔊 ambient start
        ambientAudio.Play();

        // ⏳ wait đến đoạn voice
        float waitTime = ambientAudio.clip.length - voice1.clip.length;
        if (waitTime < 0) waitTime = 0;

        yield return new WaitForSeconds(waitTime);

        // 🎤 voice1 start
        voice1.Play();

        bool triggered = false;

        while (voice1.isPlaying)
        {
            float remaining = voice1.clip.length - voice1.time;

            // 💀 khi còn 1.5s → voice2 + flash cùng lúc
            if (!triggered && remaining <= 1.2f)
            {
                triggered = true;

                // 🎤 voice2
                voice2.Play();

                // 💥 flash
                StartCoroutine(FlashEffect());

                // 📳 shake nhẹ
                if (camShake != null)
                    StartCoroutine(camShake.Shake(0.25f, 0.15f));
            }

            yield return null;
        }

        // 🖤 fade out black screen
        blackScreen.CrossFadeAlpha(0f, 1f, false);
    }

    IEnumerator FlashEffect()
    {
        // 💥 WHITE FLASH (nổ)
        whiteFlash.color = new Color(1, 1, 1, 0.7f);

        yield return new WaitForSeconds(0.2f);

        // 🌫 AFTER GLOW (màu nâu/ám vàng)
        if (blurScreen != null)
        {
            blurScreen.color = new Color(0.6f, 0.45f, 0.25f, 0.5f);
        }

        yield return new WaitForSeconds(0.7f);

        // 💥 fade white
        whiteFlash.color = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(0.2f);

        // 🖤 fade warm tint
        if (blurScreen != null)
        {
            blurScreen.color = new Color(0, 0, 0, 0);
        }
    }
}