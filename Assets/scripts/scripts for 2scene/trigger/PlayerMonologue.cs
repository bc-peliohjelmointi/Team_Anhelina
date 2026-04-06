using System.Collections;
using UnityEngine;
using TMPro;

// Yksi monologirivi: teksti ja aika, jolloin se n‰ytet‰‰n
[System.Serializable]
public class MonologueLine
{
    [TextArea] public string text; // Tekstityksen sis‰ltˆ
    public float startTime;        // Aika sekunteissa, jolloin rivi alkaa
    public float endTime;          // Aika sekunteissa, jolloin rivi loppuu
}

public class PlayerMonologue : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip clip; // ƒ‰nitiedosto, joka toistetaan

    [Header("Subtitles")]
    public MonologueLine[] lines;          // Kaikki monologirivit taulukossa
    public TextMeshProUGUI subtitleText;   // Tekstikentt‰ tekstityst‰ varten
    public float fadeSpeed = 3f;           // Tekstityksen h‰ivytyksen nopeus

    private AudioSource audioSource;              // ƒ‰nikomponentti monologille
    private bool played = false;                  // Onko monologi jo toistettu
    private CharacterController playerController; // Pelaajan liikkumiskomponentti

    void Awake()
    {
        // Luodaan ‰‰nikomponentti automaattisesti
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // Asetetaan 2D-‰‰neksi

        // Piilotetaan tekstitys pelin alussa
        if (subtitleText != null)
        {
            subtitleText.text = "";
            var c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f); // Alpha = 0, t‰ysin l‰pin‰kyv‰
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Monologi toistetaan vain kerran
        if (played) return;

        // Tarkistetaan, onko alueelle astunut objekti pelaaja
        if (!other.CompareTag("Player")) return;

        played = true;

        // Tallennetaan pelaajan liikkumiskomponentti
        playerController = other.GetComponent<CharacterController>();

        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        // Estet‰‰n pelaajan liikkuminen monologin ajaksi
        if (playerController != null)
            playerController.enabled = false;

        // Asetetaan ‰‰nileike ja aloitetaan toisto
        audioSource.clip = clip;
        audioSource.Play();

        int current = -1; // Nykyisen tekstitysrivin indeksi (-1 = ei mit‰‰n)

        // Loopataan niin kauan kuin ‰‰ni on k‰ynniss‰
        while (audioSource.isPlaying)
        {
            float t = audioSource.time; // Kulunut aika sekunteissa
            int active = -1;

            // Etsit‰‰n, mik‰ tekstitysrivi on aktiivinen t‰ll‰ hetkell‰
            for (int i = 0; i < lines.Length; i++)
            {
                if (t >= lines[i].startTime && t < lines[i].endTime)
                {
                    active = i;
                    break; // Lˆydettiin aktiivinen rivi
                }
            }

            // Jos aktiivinen rivi on vaihtunut, p‰ivitet‰‰n tekstitys
            if (active != current)
            {
                current = active;
                StopCoroutine("Fade"); // Pys‰ytet‰‰n edellinen h‰ivytys

                // N‰ytet‰‰n uusi rivi tai piilotetaan tekstitys
                StartCoroutine(Fade(current >= 0 ? lines[current].text : "", current >= 0));
            }

            yield return null; // Odotetaan seuraavaan ruutuun
        }

        // ƒ‰ni on loppunut ó piilotetaan tekstitys
        StartCoroutine(Fade("", false));

        // Palautetaan pelaajan liikkuminen
        if (playerController != null)
            playerController.enabled = true;
    }

    IEnumerator Fade(string text, bool show)
    {
        // Jos tekstikentt‰‰ ei ole, ei tehd‰ mit‰‰n
        if (subtitleText == null) yield break;

        Color c = subtitleText.color;

        // H‰ivytet‰‰n nykyinen teksti l‰pin‰kyv‰ksi
        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed * 2f; // Poistumisnopeus on kaksinkertainen
            c.a = Mathf.Max(c.a, 0f);               // Ei menn‰ alle nollan
            subtitleText.color = c;
            yield return null;
        }

        // Vaihdetaan teksti uuteen
        subtitleText.text = text;

        // Jos teksti pit‰‰ vain piilottaa, lopetetaan t‰ss‰
        if (!show) yield break;

        // H‰ivytet‰‰n uusi teksti n‰kyv‰ksi
        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Min(c.a, 1f); // Ei menn‰ yli yhden
            subtitleText.color = c;
            yield return null;
        }
    }
}