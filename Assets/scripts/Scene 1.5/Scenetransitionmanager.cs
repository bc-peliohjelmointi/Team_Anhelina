using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// handles fade-to-black transitions between scenes
// singleton so any script can call SceneTransitionManager.Instance.FadeTo("SceneName")
// put this on a persistent canvas with a full-screen black Image overlay
// the canvas should be on top of everything (high sort order)
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    // full screen black Image used for fade
    public Image fadeImage;
    // how fast the screen fades to black in seconds
    public float fadeDuration = 1.2f;
    // how long to hold black before loading new scene
    public float holdDuration = 0.3f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }
        // start fully transparent
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0);
    }

    // call this from ExitPortal or any other script to transition
    public void FadeTo(string sceneName)
    {
        StartCoroutine(FadeSequence(sceneName));
    }

    // fade out, then fade back in after load
    IEnumerator FadeSequence(string sceneName)
    {
        // fade to black
        yield return StartCoroutine(Fade(0f, 1f));
        yield return new WaitForSeconds(holdDuration);
        // load scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        // fade back in (this continues in the new scene since DontDestroyOnLoad)
        yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadeImage == null) yield break;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, a);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, to);
    }
}