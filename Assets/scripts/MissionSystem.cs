using TMPro;
using UnityEngine;

public class MissionSystem : MonoBehaviour
{
    public TextMeshProUGUI missionText;

    [System.Serializable]
    public class MissionLights
    {
        public Light[] streetLights;
    }

    public MissionLights[] missionLights; 

    int currentMission = 0;
    string[] missions =
    {
        "1.Visit Grandma;",
        "2.Deal with the thugs in the park;",
        "3.Catch the bus home..."
    };

    void Start()
    {
        UpdateUI();
        UpdateLights();
    }

    public int GetCurrentMission()
    {
        return currentMission;
    }

    public void CompleteMission(int id)
    {
        if (id == currentMission)
        {
            currentMission++;
            UpdateUI();
            UpdateLights();
        }
    }

    void UpdateLights()
    {
        for (int i = 0; i < missionLights.Length; i++)
        {
            bool isActive = (i == currentMission);
            foreach (Light light in missionLights[i].streetLights)
            {
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
                text += $"<s>{missions[i]}</s>\n";
            else if (i == currentMission)
                text += $"<color=#4B0000>{missions[i]}</color>\n"; 
            else
                text += missions[i] + "\n";
        }
        missionText.text = text;
    }
}