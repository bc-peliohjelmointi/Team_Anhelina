using UnityEngine;
using TMPro;

public class BoardHint : MonoBehaviour
{
    // Vihje-käyttöliittymäobjekti, joka näytetään pelaajalle
    public GameObject hintUI;

    void Start()
    {
        // Piilotetaan vihje pelin alussa
        hintUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        // Jos pelaaja astuu alueelle, näytetään vihje
        if (other.CompareTag("Player"))
        {
            hintUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Jos pelaaja poistuu alueelta, piilotetaan vihje
        if (other.CompareTag("Player"))
        {
            hintUI.SetActive(false);
        }
    }
}