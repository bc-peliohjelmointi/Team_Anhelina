using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleSystem : MonoBehaviour
{
    public TextMeshProUGUI subtitleText;

    public void ShowSubtitle(string text, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(SubtitleRoutine(text, duration));
    }

    IEnumerator SubtitleRoutine(string text, float duration)
    {
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        subtitleText.gameObject.SetActive(false);
    }
}