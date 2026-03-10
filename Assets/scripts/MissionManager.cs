using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MissionManager : MonoBehaviour
{
    public List<string> missions = new List<string>();
    public int currentMission = 0;

    public TextMeshProUGUI missionText;

    void Start()
    {
        missions.Add("Сходить к бабушке");
        missions.Add("Сходить к гопникам");
        missions.Add("Прийти на автобусную остановку");

        UpdateUI();
    }

    public void CompleteMission()
    {
        if (currentMission < missions.Count)
        {
            currentMission++;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        missionText.text = "";

        for (int i = 0; i < missions.Count; i++)
        {
            if (i < currentMission)
            {
                missionText.text += "• <s>" + missions[i] + "</s>\n";
            }
            else
            {
                missionText.text += "• " + missions[i] + "\n";
            }
        }
    }
}