using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    Light lt;                          // Valon komponentti tässä objektissa
    public float minIntensity = 0.8f;  // Valon pienin mahdollinen kirkkaus
    public float maxIntensity = 1.2f;  // Valon suurin mahdollinen kirkkaus
    public float flickerSpeed = 0.05f; // Kuinka usein kirkkaus vaihtuu sekunteissa

    float timer; // Ajastin seuraavaan kirkkauden vaihtoon

    void Awake()
    {
        // Haetaan valokomponentti tästä peliobjektista
        lt = GetComponent<Light>();
    }

    void Update()
    {
        // Lasketaan ajastinta alas joka ruutu
        timer -= Time.deltaTime;

        // Kun ajastin on nollassa, vaihdetaan kirkkaus
        if (timer <= 0f)
        {
            // Asetetaan satunnainen kirkkaus min- ja max-arvojen väliltä
            lt.intensity = Random.Range(minIntensity, maxIntensity);

            // Nollataan ajastin seuraavaa vilkkua varten
            timer = flickerSpeed;
        }
    }
}