using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SlideshowManager : MonoBehaviour
{
    public Image displayImage;
    public Sprite[] slides;
    public float slideDuration = 2f;
    public AudioSource music;

    private bool isPlaying = false;

    public void StartSlideshow()
    {
        if (isPlaying) return;

        isPlaying = true;
        gameObject.SetActive(true);
        StartCoroutine(PlaySlideshow());
    }

    IEnumerator PlaySlideshow()
    {
        if (music != null)
            music.Play();

        if (slides.Length == 0)
        {
            Debug.LogError("Slides array is empty!");
            yield break;
        }

        for (int i = 0; i < slides.Length; i++)
        {
            if (displayImage != null && slides[i] != null)
                displayImage.sprite = slides[i];

            yield return new WaitForSeconds(slideDuration);
        }

        RestartGame();
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
