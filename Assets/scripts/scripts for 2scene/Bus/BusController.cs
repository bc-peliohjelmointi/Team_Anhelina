using UnityEngine;

public class BusController : MonoBehaviour
{
    [Header("Mission")]
    public MissionSystem missionSystem;      // Tehtäväjärjestelmä, jota bussi seuraa
    public int triggerMissionIndex = 2;      // Tehtävän numero, jolloin bussi aktivoituu

    [Header("Lights")]
    public Light leftHeadlight;  // Vasen ajovalo
    public Light rightHeadlight; // Oikea ajovalo

    [Header("Audio")]
    public AudioSource engineAudio; // Moottorin äänikomponentti

    void Start()
    {
        // Tilataan ilmoitus tehtävän vaihtumisesta
        if (missionSystem != null)
            missionSystem.OnMissionChanged += CheckMission;

        // Piilotetaan valot pelin alussa
        SetBusActive(false);

        // Pysäytetään moottoriääni heti alussa, vaikka Play On Awake olisi päällä
        if (engineAudio != null)
            engineAudio.Stop();
    }

    void CheckMission(int currentMission)
    {
        // Tarkistetaan, onko nykyinen tehtävä bussille määritetty tehtävä
        if (currentMission == triggerMissionIndex)
        {
            ActivateBus();
        }
    }

    void ActivateBus()
    {
        // Kytketään valot päälle
        SetBusActive(true);

        // Aloitetaan moottorin äänen toisto
        if (engineAudio != null)
            engineAudio.Play();
    }

    void SetBusActive(bool state)
    {
        // Asetetaan vasemman ajovalon tila
        if (leftHeadlight != null)
            leftHeadlight.enabled = state;

        // Asetetaan oikean ajovalon tila
        if (rightHeadlight != null)
            rightHeadlight.enabled = state;
    }

    void OnDestroy()
    {
        // Peruutetaan tehtäväilmoituksen tilaus — tärkeää virheiden välttämiseksi
        if (missionSystem != null)
            missionSystem.OnMissionChanged -= CheckMission;
    }
}