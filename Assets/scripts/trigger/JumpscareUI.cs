using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// Добавь на Canvas с Image (чёрный фон + изображение монстра).
/// Скрипт сам управляет появлением через анимацию альфы.
public class JumpscareUI : MonoBehaviour
{
    public Image monsterImage;      // изображение монстра на UI
    public Image flashOverlay;      // белая вспышка при появлении
    public float flashDuration = 0.1f;
    public float fadeInTime = 0.05f;
    public float holdTime = 1.2f;
    public float fadeOutTime = 0.5f;

    void OnEnable()
    {
        StartCoroutine(PlayJumpscare());
    }

    IEnumerator PlayJumpscare()
    {
        // Белая вспышка
        if (flashOverlay != null)
        {
            flashOverlay.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(flashDuration);
            flashOverlay.color = new Color(1, 1, 1, 0);
        }

        // Плавное появление монстра
        if (monsterImage != null)
        {
            float t = 0;
            while (t < fadeInTime)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Clamp01(t / fadeInTime);
                monsterImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
        }

        yield return new WaitForSeconds(holdTime);

        // Угасание
        if (monsterImage != null)
        {
            float t = 0;
            while (t < fadeOutTime)
            {
                t += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(t / fadeOutTime);
                monsterImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
        }
    }
}