using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BusEndingTrigger : MonoBehaviour
{
    [Header("Настройки")]
    public float fadeDuration = 2f;          // Ruudun häivytyksen kesto sekunteissa
    public float displayTime = 60f;          // Kuinka kauan lopputeksti näkyy ruudulla
    public string endingText = "Продолжение следует"; // Lopputekstin sisältö
    public int mainMenuSceneIndex = 0;       // Päävalikon scenen numero

    [Header("Звук")]
    public AudioClip endingMusic;            // Loppumusiikki
    public float musicFadeInDuration = 2f;   // Kuinka kauan musiikin äänenvoimakkuus nousee
    public float soundFadeOutDuration = 1f;  // Kuinka kauan muut äänet häivytetään

    [Header("UI (можно задать своё или создастся автоматически)")]
    public Canvas endingCanvas;              // Kanvas, johon lopputeksti piirretään
    public Image fadePanel;                  // Musta paneeli ruudun peittämiseen
    public TextMeshProUGUI endingLabel;      // Lopputekstin tekstikenttä

    private bool _triggered = false;         // Onko loppusekvenssi jo käynnistetty
    private AudioSource _endingAudioSource;  // Loppumusiikin äänikomponentti

    private void OnTriggerEnter(Collider other)
    {
        // Jos loppusekvenssi on jo käynnistynyt, ei tehdä mitään
        if (_triggered) return;

        // Tarkistetaan, onko törmäävä objekti pelaaja
        if (!other.CompareTag("Player")) return;

        // Haetaan tehtäväjärjestelmä scenestä
        MissionSystem missionSystem = FindObjectOfType<MissionSystem>();

        // Loppusekvenssi käynnistyy vain, jos viimeinen tehtävä on aktiivinen
        if (missionSystem == null || !missionSystem.IsLastMission()) return;

        _triggered = true;
        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        // Luodaan UI-elementit ja äänikomponentti
        SetupUI();
        SetupAudio();

        // 1. Häivytetään kaikki äänet ja aloitetaan loppumusiikki
        StartCoroutine(FadeOutAllSounds(soundFadeOutDuration));
        StartCoroutine(FadeInEndingMusic(musicFadeInDuration));

        // 2. Pimitetään ruutu mustaksi häivyttämällä
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration); // Lasketaan läpinäkymättömyys (0 → 1)
            fadePanel.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        // Varmistetaan, että ruutu on täysin musta
        fadePanel.color = Color.black;

        // 3. Näytetään lopputeksti häivyttämällä se esiin
        endingLabel.gameObject.SetActive(true);
        t = 0f;
        float textFadeDuration = 1.5f;
        while (t < textFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / textFadeDuration); // Lasketaan tekstin näkyvyys (0 → 1)
            endingLabel.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        // 4. Odotetaan ennen päävalikkoon siirtymistä
        yield return new WaitForSeconds(displayTime);

        // 5. Ladataan päävalikko
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void SetupAudio()
    {
        // Luodaan uusi äänikomponentti loppumusiikille
        _endingAudioSource = gameObject.AddComponent<AudioSource>();
        _endingAudioSource.clip = endingMusic;
        _endingAudioSource.loop = true;      // Musiikki toistuu silmukassa
        _endingAudioSource.volume = 0f;      // Aloitetaan hiljaa, äänenvoimakkuus nousee myöhemmin
        _endingAudioSource.Play();
    }

    // Häivytetään kaikki scenessä olevat äänet hiljaiseksi, paitsi loppumusiikki
    private IEnumerator FadeOutAllSounds(float duration)
    {
        // Haetaan kaikki äänikomponentit scenestä
        AudioSource[] allSources = FindObjectsOfType<AudioSource>();

        // Tallennetaan jokaisen äänikomponentin alkuperäinen äänenvoimakkuus
        float[] startVolumes = new float[allSources.Length];
        for (int i = 0; i < allSources.Length; i++)
            startVolumes[i] = allSources[i].volume;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float factor = 1f - Mathf.Clamp01(t / duration); // Lasketaan voimakkuuskerroin (1 → 0)

            for (int i = 0; i < allSources.Length; i++)
            {
                // Ohitetaan loppumusiikin äänikomponentti
                if (allSources[i] == _endingAudioSource) continue;

                // Lasketaan äänenvoimakkuutta tasaisesti
                if (allSources[i] != null)
                    allSources[i].volume = startVolumes[i] * factor;
            }

            yield return null;
        }

        // Pysäytetään kaikki muut äänet kokonaan
        foreach (var source in allSources)
        {
            if (source == _endingAudioSource) continue;
            if (source != null) source.Stop();
        }
    }

    // Nostetaan loppumusiikin äänenvoimakkuus tasaisesti täyteen
    private IEnumerator FadeInEndingMusic(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _endingAudioSource.volume = Mathf.Clamp01(t / duration); // Äänenvoimakkuus nousee (0 → 1)
            yield return null;
        }
        // Varmistetaan, että äänenvoimakkuus on täynnä
        _endingAudioSource.volume = 1f;
    }

    private void SetupUI()
    {
        // Jos kanvasta ei ole asetettu, luodaan se automaattisesti
        if (endingCanvas == null)
        {
            GameObject canvasObj = new GameObject("EndingCanvas");
            endingCanvas = canvasObj.AddComponent<Canvas>();
            endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // Piirretään ruudun päälle
            endingCanvas.sortingOrder = 100; // Näytetään kaiken muun päällä
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Jos häivytyspaneelia ei ole, luodaan se automaattisesti
        if (fadePanel == null)
        {
            GameObject panelObj = new GameObject("FadePanel");
            panelObj.transform.SetParent(endingCanvas.transform, false);
            fadePanel = panelObj.AddComponent<Image>();
            fadePanel.color = new Color(0f, 0f, 0f, 0f); // Täysin läpinäkyvä aluksi

            // Venytetään paneeli koko ruudun kokoiseksi
            RectTransform rt = fadePanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        // Varmistetaan, että paneeli on näkyvissä
        fadePanel.gameObject.SetActive(true);

        // Jos lopputekstikenttää ei ole, luodaan se automaattisesti
        if (endingLabel == null)
        {
            GameObject textObj = new GameObject("EndingText");
            textObj.transform.SetParent(endingCanvas.transform, false);
            endingLabel = textObj.AddComponent<TextMeshProUGUI>();
            endingLabel.text = endingText;
            endingLabel.fontSize = 48;
            endingLabel.alignment = TextAlignmentOptions.Center; // Teksti keskitetään
            endingLabel.color = new Color(1f, 1f, 1f, 0f); // Täysin läpinäkyvä aluksi

            // Venytetään tekstikenttä koko ruudun kokoiseksi
            RectTransform rt = endingLabel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

        // Piilotetaan lopputeksti — se näytetään myöhemmin häivyttämällä
        endingLabel.gameObject.SetActive(false);
    }
}