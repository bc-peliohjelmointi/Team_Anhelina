using System.Collections;
using UnityEngine;

public class BoardInteraction : MonoBehaviour
{
    [Header("Player")]
    public MonoBehaviour playerController; // Pelaajan liikkumiskomponentti
    public Camera playerCamera;            // Pelaajan kamera

    [Header("Camera View")]
    public Transform cameraPoint;  // Kameran sijainti laudan katselutilassa
    public float viewTime = 60f;   // Kuinka kauan katselutila kestää sekunteissa

    [Header("Dialogue")]
    public DialoguePlayer dialogue; // Dialogisoitin, joka toistaa puheen

    [Header("Map")]
    public Renderer mapRenderer;          // Kartan renderöijä, jolla vaihdetaan tekstuuria
    public Texture[] missionTextures;     // Tehtävien karttakuvat taulukossa
    public MissionSystem missionSystem;   // Tehtäväjärjestelmä, josta haetaan nykyinen tehtävä

    private bool playerNear = false;  // Onko pelaaja laudan lähellä
    private bool isViewing = false;   // Onko katselutila aktiivisena

    private Vector3 oldCamPos;      // Kameran vanha sijainti ennen katselutilaa
    private Quaternion oldCamRot;   // Kameran vanha suunta ennen katselutilaa

    public float fadeDuration = 1f; // Tekstuurin vaihdon häivytyksen kesto sekunteissa

    void Start()
    {
        // Piilotetaan vuorovaikutusvihje pelin alussa
        InteractionHint.instance.Hide();
    }

    void Update()
    {
        // Tarkistetaan joka ruutu, painaako pelaaja M-näppäintä
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!isViewing)
                // Ei katselutilassa — siirrytään katselutilaan
                EnterView();
            else
            {
                // Ollaan katselutilassa — pysäytetään dialogi ja poistutaan tilasta
                if (dialogue != null)
                    dialogue.Stop();
                ExitView();
            }
        }
    }

    IEnumerator FadeTexture(Texture newTexture)
    {
        Material mat = mapRenderer.material; // Haetaan kartan materiaali

        // Häivytetään vanha tekstuuri läpinäkyväksi
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / fadeDuration); // Lasketaan läpinäkyvyys (1 → 0)
            mat.color = new Color(1, 1, 1, alpha);
            yield return null; // Odotetaan seuraavaan ruutuun
        }

        // Vaihdetaan tekstuuri uuteen
        mat.mainTexture = newTexture;

        // Häivytetään uusi tekstuuri näkyväksi
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration; // Lasketaan läpinäkyvyys (0 → 1)
            mat.color = new Color(1, 1, 1, alpha);
            yield return null; // Odotetaan seuraavaan ruutuun
        }
    }

    void EnterView()
    {
        isViewing = true;

        // Piilotetaan vuorovaikutusvihje katselutilan aikana
        InteractionHint.instance.Hide();

        // Tallennetaan kameran nykyinen sijainti ja suunta palautusta varten
        oldCamPos = playerCamera.transform.position;
        oldCamRot = playerCamera.transform.rotation;

        // Estetään pelaajan liikkuminen katselutilan aikana
        if (playerController != null)
            playerController.enabled = false;

        // Siirretään kamera laudan katselupisteeseen
        playerCamera.transform.position = cameraPoint.position;
        playerCamera.transform.rotation = cameraPoint.rotation;

        // Päivitetään kartan tekstuuri nykyisen tehtävän mukaan
        UpdateMapTexture();

        // Aloitetaan dialogin toisto
        if (dialogue != null)
            dialogue.Play();

        // Käynnistetään ajastin, joka sulkee katselutilan automaattisesti
        StartCoroutine(ViewTimer());
    }

    IEnumerator ViewTimer()
    {
        float timer = 0f;

        // Odotetaan niin kauan kuin dialogi on käynnissä tai ajastin ei ole loppunut
        while (timer < viewTime)
        {
            // Jos dialogi on päättynyt, poistutaan katselutilasta heti
            if (dialogue != null && !dialogue.IsPlaying())
                break;

            timer += Time.deltaTime;
            yield return null; // Odotetaan seuraavaan ruutuun
        }

        // Suljetaan katselutila ajastimen tai dialogin loputtua
        ExitView();
    }

    void ExitView()
    {
        // Jos ei olla katselutilassa, ei tehdä mitään
        if (!isViewing) return;

        isViewing = false;

        // Palautetaan pelaajan liikkuminen käyttöön
        if (playerController != null)
            playerController.enabled = true;

        // Palautetaan kamera alkuperäiseen sijaintiin ja suuntaan
        playerCamera.transform.position = oldCamPos;
        playerCamera.transform.rotation = oldCamRot;

        // Jos pelaaja on edelleen laudan lähellä, näytetään vihje uudelleen
        if (playerNear)
            InteractionHint.instance.Show("Press M to view map");
    }

    void UpdateMapTexture()
    {
        // Jos tehtäväjärjestelmä tai kartta puuttuu, ei tehdä mitään
        if (missionSystem == null || mapRenderer == null) return;

        int mission = missionSystem.GetCurrentMission(); // Haetaan nykyisen tehtävän numero

        // Tarkistetaan, että tehtävälle on olemassa tekstuuri taulukossa
        if (mission >= 0 && mission < missionTextures.Length)
            StartCoroutine(FadeTexture(missionTextures[mission])); // Vaihdetaan tekstuuri häivyttämällä
    }

    void OnTriggerEnter(Collider other)
    {
        // Tarkistetaan, onko törmäävä objekti pelaaja
        if (!other.CompareTag("Player")) return;

        playerNear = true;

        // Näytetään vihje vain, jos katselutila ei ole aktiivisena
        if (!isViewing)
            InteractionHint.instance.Show("Press M to view map");
    }

    void OnTriggerExit(Collider other)
    {
        // Tarkistetaan, onko poistuva objekti pelaaja
        if (!other.CompareTag("Player")) return;

        playerNear = false;

        // Piilotetaan vihje, kun pelaaja poistuu laudan luota
        InteractionHint.instance.Hide();
    }
}