using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // Tuodaan kokoelmat käyttöön (tarvitaan IEnumerator-tyypille)

public class CreditsSlideshow : MonoBehaviour
{
    // Lista kaikista ruuduista, jotka näytetään
    public GameObject[] slides;
    public float slideDuration = 3f;
    // Muistetaan, mikä ruutu näytetään nyt
    private int currentIndex = 0;

    //aloittaa esityksen
    public void StartSlideshow()
    {
        StartCoroutine(ShowSlides());
    }

    //näyttää ruudut yksi kerrallaan
    IEnumerator ShowSlides()
    {
        // Käydään läpi kaikki ruudut järjestyksessä
        while (currentIndex < slides.Length)
        {
     
            slides[currentIndex].SetActive(true); // nykyinen ruutu
            yield return new WaitForSecondsRealtime(slideDuration);   // Odotetaan, kunnes on aika vaihtaa seuraavaan ruutu
            slides[currentIndex].SetActive(false);
            currentIndex++; // Siirrytään seuraavaan ruutuun
        }

        // Palautetaan pelin nopeus normaaliksi
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Ladataan nykyinen kohtaus uudelleen alusta
    }
}