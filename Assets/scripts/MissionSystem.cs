using TMPro;
using UnityEngine;

public class MissionSystem : MonoBehaviour
{
    public TextMeshProUGUI missionText;

    int currentMission = 0;

    string[] missions =
    {
        "Visit Grandma",
        "Deal with the thugs in the park",
        "Catch the bus home"
    };

    void Start()
    {
        UpdateUI();
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
        }
    }

    void UpdateUI()
    {
        string text = "<b>TASKS</b>\n\n";

        for (int i = 0; i < missions.Length; i++)
        {
            if (i < currentMission)
            {
                text += $"<s>{missions[i]}</s>\n";
            }
            else if (i == currentMission)
            {
                text += $"<color=red>{missions[i]}</color>\n";
            }
            else
            {
                text += missions[i] + "\n";
            }
        }

        missionText.text = text;
    }
}