using TMPro;
using UnityEngine;
using System;

public class MissionSystem : MonoBehaviour
{
    public TextMeshProUGUI missionText; // Tekstikenttä, johon tehtävälista näytetään

    // Sisäinen luokka, joka tallentaa yhden tehtävän katuvalot
    [System.Serializable]
    public class MissionLights
    {
        public Light[] streetLights; // Katuvalot, jotka kuuluvat tähän tehtävään
    }

    public MissionLights[] missionLights;   // Katuvaloryhmät jokaiselle tehtävälle
    public event Action<int> OnMissionChanged; // Tapahtuma, joka laukeaa kun tehtävä vaihtuu

    int currentMission = 0; // Nykyisen tehtävän numero (alkaa nollasta)

    // Kaikki tehtävät tekstinä taulukossa
    string[] missions =
    {
        "1.Visit Grandma;",
        "2.Deal with the thugs in the park;",
        "3.Catch the bus home..."
    };

    void Start()
    {
        // Päivitetään tehtävälista ja valot pelin alussa
        UpdateUI();
        UpdateLights();
    }

    // Palauttaa nykyisen tehtävän numeron
    public int GetCurrentMission()
    {
        return currentMission;
    }

    // Tarkistaa, onko viimeinen tehtävä aktiivisena
    public bool IsLastMission()
    {
        return currentMission == missions.Length - 1;
    }

    public void CompleteMission(int id)
    {
        // Voidaan suorittaa vain nykyinen tehtävä — ei vanhoja eikä tulevia
        if (id == currentMission)
        {
            currentMission++; // Siirrytään seuraavaan tehtävään

            // Ilmoitetaan kaikille kuuntelijoille, että tehtävä on vaihtunut
            OnMissionChanged?.Invoke(currentMission);

            // Päivitetään käyttöliittymä ja valot uuden tehtävän mukaan
            UpdateUI();
            UpdateLights();
        }
    }

    void UpdateLights()
    {
        // Käydään läpi kaikki tehtävien valoryhmät
        for (int i = 0; i < missionLights.Length; i++)
        {
            // Vain nykyisen tehtävän valot ovat päällä
            bool isActive = (i == currentMission);

            foreach (Light light in missionLights[i].streetLights)
            {
                // Kytketään valo päälle tai pois tehtävän mukaan
                if (light != null)
                    light.enabled = isActive;
            }
        }
    }

    void UpdateUI()
    {
        string text = "<b>TASKS</b>\n\n";

        for (int i = 0; i < missions.Length; i++)
        {
            if (i < currentMission)
                // Suoritettu tehtävä näytetään yliviivattuna
                text += $"<s>{missions[i]}</s>\n";
            else if (i == currentMission)
                // Nykyinen tehtävä korostetaan tummanpunaisella värillä
                text += $"<color=#4B0000>{missions[i]}</color>\n";
            else
                // Tuleva tehtävä näytetään normaalisti
                text += missions[i] + "\n";
        }

        // Päivitetään tekstikenttä uudella sisällöllä
        missionText.text = text;
    }
}