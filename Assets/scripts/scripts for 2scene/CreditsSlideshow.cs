using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditsSlideshow : MonoBehaviour
{
    public GameObject[] slides;       // Kaikki loppukreditit diaesityksenä taulukossa
    public float slideDuration = 3f;  // Kuinka kauan yksi dia näkyy sekunteissa

    private int currentIndex = 0; // Nykyisen dian indeksi

    // Käynnistää diaesityksen ulkopuolelta kutsuttaessa
    public void StartSlideshow()
    {
        StartCoroutine(ShowSlides());
    }

    IEnumerator ShowSlides()
    {
        // Käydään läpi kaikki diat järjestyksessä
        while (currentIndex < slides.Length)
        {
            // Näytetään nykyinen dia
            slides[currentIndex].SetActive(true);

            // Odotetaan dian näyttöaika — Realtime toimii myös kun peli on pysäytetty
            yield return new WaitForSecondsRealtime(slideDuration);

            // Piilotetaan dia ennen seuraavaan siirtymistä
            slides[currentIndex].SetActive(false);

            currentIndex++;
        }

        // Palautetaan pelin aikanopeus normaaliksi, jos se oli pysäytettynä
        Time.timeScale = 1f;

        // Ladataan nykyinen scene uudelleen — palaa pelin alkuun
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}