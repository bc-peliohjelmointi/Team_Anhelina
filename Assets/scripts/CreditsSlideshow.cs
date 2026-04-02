using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsSlideshow : MonoBehaviour
{
    public GameObject[] slides;
    public float slideDuration = 3f;

    private int currentIndex = 0;

    public void StartSlideshow()
    {
        StartCoroutine(ShowSlides());
    }

    IEnumerator ShowSlides()
    {
        while (currentIndex < slides.Length)
        {
            slides[currentIndex].SetActive(true);

            yield return new WaitForSecondsRealtime(slideDuration);

            slides[currentIndex].SetActive(false);
            currentIndex++;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}