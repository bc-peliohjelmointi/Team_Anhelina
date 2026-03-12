using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public string[] missions;
    public int currentMission = 0;
    public TextMeshProUGUI missionText;

    [System.Serializable]
    public class MissionLights
    {
        public string missionName;
        public Light[] streetLights;
    }

    public MissionLights[] missionLights; 

    void Start()
    {
        UpdateLights();
        UpdateUI();
    }

    public void CompleteMission()
    {
        currentMission++;
        if (currentMission >= missions.Length)
            currentMission = missions.Length - 1;

        UpdateLights();
        UpdateUI();
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
        missionText.text = "";
        for (int i = 0; i < missions.Length; i++)
            missionText.text += (i < currentMission
                ? "• <s>" + missions[i] + "</s>\n"
                : "• " + missions[i] + "\n");
    }
}