using System.Collections;
using UnityEngine;
using TMPro;

// Yksi tekstitysrivi: teksti ja aika, jolloin se näytetään
[System.Serializable]
public class DialogueSubtitle
{
    [TextArea] public string text; // Tekstityksen sisältö
    public float startTime;        // Aika sekunteissa, jolloin rivi alkaa
    public float endTime;          // Aika sekunteissa, jolloin rivi loppuu
}

public class NPCInteraction : MonoBehaviour
{
    [Header("Mission Settings")]
    public int missionID;               // Tehtävän numero, johon tämä NPC liittyy
    public MissionSystem missionSystem; // Viittaus tehtäväjärjestelmään

    [Header("Dialogue Settings")]
    public AudioClip dialogueAudio;        // Dialogin äänitiedosto
    public DialogueSubtitle[] subtitles;   // Kaikki tekstitysrivit taulukossa
    public TextMeshProUGUI subtitleText;   // Tekstikenttä tekstitystä varten
    public float fadeSpeed = 3f;           // Tekstityksen häivytyksen nopeus

    [Header("Trigger Mode")]
    public bool autoPlayOnEnter = false; // Jos true, dialogi alkaa automaattisesti alueelle astuessa

    private AudioSource audioSource;              // Äänikomponentti dialogille
    private bool playerNear = false;              // Onko pelaaja NPC:n lähellä
    private bool isTalking = false;               // Onko dialogi käynnissä
    private CharacterController playerController; // Pelaajan liikkumiskomponentti

    void Awake()
    {
        // Luodaan äänikomponentti automaattisesti
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // Asetetaan 2D-ääneksi

        // Piilotetaan tekstitys pelin alussa
        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f); // Alpha = 0, täysin läpinäkyvä
        }
    }

    void Update()
    {
        // Tarkistetaan, painaako pelaaja E-näppäintä NPC:n lähellä
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isTalking && !autoPlayOnEnter)
        {
            // Aloitetaan dialogi vain, jos oikea tehtävä on aktiivisena
            if (missionSystem == null || missionSystem.GetCurrentMission() == missionID)
            {
                StartCoroutine(PlayDialogue());

                // Merkitään tehtävä suoritetuksi dialogin alkaessa
                if (missionSystem != null)
                    missionSystem.CompleteMission(missionID);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Tarkistetaan, onko alueelle astunut objekti pelaaja
        if (!other.CompareTag("Player")) return;

        playerNear = true;

        // Tallennetaan pelaajan liikkumiskomponentti
        playerController = other.GetComponent<CharacterController>();

        // Näytetään vihje vain manuaalisessa tilassa
        if (!isTalking && !autoPlayOnEnter)
            InteractionHint.instance.Show("Press E to talk");

        // Automaattisessa tilassa dialogi alkaa heti alueelle astuessa
        if (autoPlayOnEnter && !isTalking)
            StartCoroutine(PlayDialogue());
    }

    void OnTriggerExit(Collider other)
    {
        // Tarkistetaan, onko alueelta poistunut objekti pelaaja
        if (!other.CompareTag("Player")) return;

        playerNear = false;
        playerController = null;

        // Piilotetaan vihje ja pysäytetään dialogi
        InteractionHint.instance.Hide();
        StopDialogue();
    }

    void SetPlayerMovement(bool enabled)
    {
        // Kytketään pelaajan liikkuminen päälle tai pois
        if (playerController != null)
            playerController.enabled = enabled;
    }

    void StopDialogue()
    {
        // Jos dialogi ei ole käynnissä, ei tehdä mitään
        if (!isTalking) return;

        // Pysäytetään kaikki coroutiinit ja ääni
        StopAllCoroutines();
        audioSource.Stop();
        isTalking = false;

        // Palautetaan pelaajan liikkuminen
        SetPlayerMovement(true);

        // Tyhjennetään tekstityskenttä
        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }
    }

    IEnumerator PlayDialogue()
    {
        isTalking = true;

        // Piilotetaan vihje dialogin ajaksi
        InteractionHint.instance.Hide();

        // Estetään pelaajan liikkuminen dialogin ajaksi
        SetPlayerMovement(false);

        // Aloitetaan äänen toisto
        if (dialogueAudio != null)
        {
            audioSource.clip = dialogueAudio;
            audioSource.Play();
        }

        int currentSubtitle = -1; // Nykyisen tekstitysrivin indeksi (-1 = ei mitään)

        // Loopataan niin kauan kuin ääni on käynnissä
        while (audioSource.isPlaying)
        {
            float elapsed = audioSource.time; // Kulunut aika sekunteissa
            int activeLine = -1;

            // Etsitään, mikä tekstitysrivi on aktiivinen tällä hetkellä
            for (int i = 0; i < subtitles.Length; i++)
            {
                if (elapsed >= subtitles[i].startTime && elapsed < subtitles[i].endTime)
                {
                    activeLine = i;
                    break; // Löydettiin aktiivinen rivi
                }
            }

            // Jos aktiivinen rivi on vaihtunut, päivitetään tekstitys
            if (activeLine != currentSubtitle)
            {
                currentSubtitle = activeLine;
                StopCoroutine("FadeText"); // Pysäytetään edellinen häivytys

                if (currentSubtitle >= 0)
                    // Näytetään uusi tekstitysrivi
                    StartCoroutine(FadeText(subtitles[currentSubtitle].text, true));
                else
                    // Ei aktiivista riviä — piilotetaan tekstitys
                    StartCoroutine(FadeText("", false));
            }

            yield return null; // Odotetaan seuraavaan ruutuun
        }

        // Ääni on loppunut — piilotetaan tekstitys
        StartCoroutine(FadeText("", false));

        // Palautetaan pelaajan liikkuminen
        SetPlayerMovement(true);
        isTalking = false;

        // Näytetään vihje uudelleen, jos pelaaja on edelleen lähellä
        if (playerNear)
            InteractionHint.instance.Show("Press E to talk");
    }

    IEnumerator FadeText(string text, bool show)
    {
        // Jos tekstikenttää ei ole, ei tehdä mitään
        if (subtitleText == null) yield break;

        Color c = subtitleText.color;

        // Häivytetään nykyinen teksti läpinäkyväksi
        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed * 2f;      // Poistumisnopeus on kaksinkertainen
            c.a = Mathf.Max(c.a, 0f);                    // Ei mennä alle nollan
            subtitleText.color = c;
            yield return null;
        }

        // Vaihdetaan teksti uuteen
        subtitleText.text = text;

        // Jos teksti pitää vain piilottaa, lopetetaan tässä
        if (!show) yield break;

        // Häivytetään uusi teksti näkyväksi
        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Min(c.a, 1f);  // Ei mennä yli yhden
            subtitleText.color = c;
            yield return null;
        }
    }
}