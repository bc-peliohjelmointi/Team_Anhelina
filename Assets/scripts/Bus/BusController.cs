using UnityEngine;

public class BusController : MonoBehaviour
{
    [Header("Mission")]
    public MissionSystem missionSystem;
    public int triggerMissionIndex = 2;

    [Header("Lights")]
    public Light leftHeadlight;
    public Light rightHeadlight;

    [Header("Audio")]
    public AudioSource engineAudio;

    void Start()
    {
        // подписка на событие
        if (missionSystem != null)
            missionSystem.OnMissionChanged += CheckMission;

        // выключаем автобус
        SetBusActive(false);
    }

    void CheckMission(int currentMission)
    {
        if (currentMission == triggerMissionIndex)
        {
            ActivateBus();
        }
    }

    void ActivateBus()
    {
        SetBusActive(true);

        if (engineAudio != null)
            engineAudio.Play();
    }

    void SetBusActive(bool state)
    {
        if (leftHeadlight != null)
            leftHeadlight.enabled = state;

        if (rightHeadlight != null)
            rightHeadlight.enabled = state;
    }

    void OnDestroy()
    {
        // отписка (важно, чтобы не было багов)
        if (missionSystem != null)
            missionSystem.OnMissionChanged -= CheckMission;
    }
}