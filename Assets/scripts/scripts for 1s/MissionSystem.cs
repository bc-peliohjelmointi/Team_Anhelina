using TMPro;
using UnityEngine;
using System;

public class MissionSystem : MonoBehaviour
{
    public TextMeshProUGUI missionText;
    // Tämä luokka sisältää yhden tehtävän lamput
    [System.Serializable]
    public class MissionLights
    {
        public Light[] streetLights;  // Lista katulamppuja
    }

    // Lista kaikista tehtävien lampuista
    public MissionLights[] missionLights;

    // Tapahtuma, joka laukeaa kun tehtävä vaihtuu
    public event Action<int> OnMissionChanged;

    int currentMission = 0; // Muistetaan, mikä tehtävä on nyt käynnissä

    // Kaikki tehtävät tekstinä
    string[] missions =
    {
        "1.Visit Grandma;",
        "2.Deal with the thugs in the park;",
        "3.Catch the bus home..."
    };

    // suoritetaan heti pelin alussa
    void Start()
    {
        // Päivitetään tehtäväteksti ruudulle
        UpdateUI();
        // Päivitetään lamput oikein
        UpdateLights();
    }

    // Palauttaa nykyisen tehtävän numeron
    public int GetCurrentMission()
    {
        return currentMission;
    }

    // Tarkistaa, onko tämä viimeinen tehtävä
    public bool IsLastMission()
    {
        return currentMission == missions.Length - 1;
    }

    // Merkitsee tehtävän valmiiksi, jos se on oikea tehtävä
    public void CompleteMission(int id)
    {
        if (id == currentMission)
        {
            currentMission++;// Siirrytään seuraavaan tehtävään
            OnMissionChanged?.Invoke(currentMission); // Ilmoitetaan muille, että tehtävä vaihtui

            // Päivitetään teksti ruudulle, lamput uuden tehtävän mukaan
            UpdateUI();
            UpdateLights();
        }
    }

    // Päivittää lamput sen mukaan, mikä tehtävä on käynnissä
    void UpdateLights()
    {
        // Käydään läpi kaikki tehtävien lampuryhmät
        for (int i = 0; i < missionLights.Length; i++)
        {
            bool isActive = (i == currentMission); // Lamppu on päällä vain nykyisessä tehtävässä

            // Käydään läpi kaikki lamput tässä ryhmässä
            foreach (Light light in missionLights[i].streetLights)
            {
                // Tarkistetaan, että lamppu on olemassa
                if (light != null)
                    // Laitetaan lamppu päälle tai pois
                    light.enabled = isActive;
            }
        }
    }

    // Päivittää tehtävälistan tekstin ruudulle
    void UpdateUI()
    {
        // Aloitetaan teksti otsikolla
        string text = "<b>TASKS</b>\n\n";
        // Käydään läpi kaikki tehtävät
        for (int i = 0; i < missions.Length; i++)
        {
            // Valmis tehtävä näytetään yliviivattuna
            if (i < currentMission)
                text += $"<s>{missions[i]}</s>\n";
            // Nykyinen tehtävä näytetään punaisella värillä
            else if (i == currentMission)
                text += $"<color=#4B0000>{missions[i]}</color>\n";
            // Tuleva tehtävä näytetään normaalisti
            else
                text += missions[i] + "\n";
        }
        // Asetetaan valmis teksti ruudulle
        missionText.text = text;
    }
}