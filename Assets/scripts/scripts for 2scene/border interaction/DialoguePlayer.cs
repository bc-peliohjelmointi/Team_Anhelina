using System.Collections;
using UnityEngine;
using TMPro;

// Yksi dialogirivi: teksti ja aika, jolloin se n‰ytet‰‰n
[System.Serializable]
public class DialogueLine
{
    [TextArea] public string text;  // Tekstitys, joka n‰ytet‰‰n ruudulla
    public float startTime;         // Aika sekunteissa, jolloin rivi alkaa
    public float endTime;           // Aika sekunteissa, jolloin rivi loppuu
}

public class DialoguePlayer : MonoBehaviour
{
    public AudioClip audioClip;         // ƒ‰nitiedosto, joka toistetaan
    public DialogueLine[] lines;        // Kaikki dialogirivit taulukossa
    public TextMeshProUGUI subtitleText; // Tekstielementti, johon tekstitys n‰ytet‰‰n
    public float fadeSpeed = 3f;        // Kuinka nopeasti teksti ilmestyy ja h‰ipyy

    AudioSource audioSource; // ƒ‰nikomponentti, joka toistaa ‰‰nen
    bool isPlaying = false;  // Onko dialogi t‰ll‰ hetkell‰ k‰ynniss‰

    void Awake()
    {
        // Luodaan ‰‰nikomponentti automaattisesti t‰h‰n peliobjektiin
        audioSource = gameObject.AddComponent<AudioSource>();
        // Asetetaan ‰‰ni 2D-‰‰neksi (ei sijainnista riippuvainen)
        audioSource.spatialBlend = 0f;

        // Piilotetaan tekstityskentt‰ pelin alussa
        if (subtitleText != null)
        {
            subtitleText.text = "";
            var c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f); // Alpha = 0, t‰ysin l‰pin‰kyv‰
        }
    }

    public void Play()
    {
        // Jos dialogi on jo k‰ynniss‰, ei aloiteta uudelleen
        if (isPlaying) return;
        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        isPlaying = true;

        // Asetetaan ‰‰nileike ja aloitetaan toisto
        audioSource.clip = audioClip;
        audioSource.Play();

        int current = -1; // Nykyisen aktiivisen dialogirivin indeksi (-1 = ei mit‰‰n)

        // Loopataan niin kauan kuin ‰‰ni on k‰ynniss‰
        while (audioSource.isPlaying)
        {
            float t = audioSource.time; // Haetaan ‰‰ness‰ kulunut aika
            int active = -1;            // Etsit‰‰n, mik‰ rivi on aktiivinen t‰ll‰ hetkell‰

            for (int i = 0; i < lines.Length; i++)
            {
                // Tarkistetaan, onko nykyinen aika t‰m‰n rivin aikav‰lin sis‰ll‰
                if (t >= lines[i].startTime && t < lines[i].endTime)
                {
                    active = i;
                    break; // Lˆydettiin aktiivinen rivi, ei tarvitse jatkaa hakua
                }
            }

            // Jos aktiivinen rivi on vaihtunut, p‰ivitet‰‰n tekstitys
            if (active != current)
            {
                current = active;
                StopCoroutine("FadeText"); // Pys‰ytet‰‰n mahdollinen edellinen h‰ivytys

                if (current >= 0)
                    // N‰ytet‰‰n uusi tekstitysrivi h‰ivytt‰m‰ll‰ se esiin
                    StartCoroutine(FadeText(lines[current].text, true));
                else
                    // Ei aktiivista rivi‰ ó piilotetaan tekstitys
                    StartCoroutine(FadeText("", false));
            }

            yield return null; // Odotetaan seuraavaan ruutuun
        }

        // ƒ‰ni on loppunut ó piilotetaan tekstitys
        StartCoroutine(FadeText("", false));
        isPlaying = false;
    }

    IEnumerator FadeText(string text, bool show)
    {
        // Jos tekstikentt‰‰ ei ole, ei tehd‰ mit‰‰n
        if (subtitleText == null) yield break;

        Color c = subtitleText.color;

        // H‰ivytet‰‰n nykyinen teksti l‰pin‰kyv‰ksi
        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed * 2f; // Poistumisnopeus on kaksinkertainen
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
            subtitleText.color = c;
            yield return null;
        }
    }

    // Palauttaa tiedon, onko dialogi k‰ynniss‰
    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void Stop()
    {
        // Jos dialogi ei ole k‰ynniss‰, ei tehd‰ mit‰‰n
        if (!isPlaying) return;

        // Pys‰ytet‰‰n kaikki k‰ynniss‰ olevat coroutiinit
        StopAllCoroutines();

        // Pys‰ytet‰‰n ‰‰ni
        audioSource.Stop();
        isPlaying = false;

        // Tyhjennet‰‰n tekstityskentt‰ ja piilotetaan se v‰littˆm‰sti
        if (subtitleText != null)
        {
            subtitleText.text = "";
            var c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f); // Alpha = 0, t‰ysin l‰pin‰kyv‰
        }
    }
}