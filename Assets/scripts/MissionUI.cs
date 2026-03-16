using TMPro;
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    public TextMeshProUGUI missionText;

    public void SetMission(string text)
    {
        missionText.text = text;
    }

    public void AddMission(string mission)
    {
        missionText.text += "\n• " + mission;
    }
}