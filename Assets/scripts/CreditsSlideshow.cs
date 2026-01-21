using UnityEngine;
using System.Collections;

public class CreditsSlideshow : MonoBehaviour
{
    public GameObject[] slides; // сюда перетащи картинки в инспекторе
    public float slideDuration = 5f; // сколько секунд показывать каждую картинку

    private int currentIndex = 0;

    public void StartSlideshow()
    {
        StartCoroutine(ShowSlides());
    }

    IEnumerator ShowSlides()
    {
        while (currentIndex < slides.Length)
        {
            // Включаем текущий слайд
            slides[currentIndex].SetActive(true);

            // Ждём нужное время
            yield return new WaitForSeconds(slideDuration);

            // Выключаем текущий и переходим к следующему
            slides[currentIndex].SetActive(false);
            currentIndex++;
        }

        // Когда все картинки показаны — можно перезапустить игру
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
    }
}
