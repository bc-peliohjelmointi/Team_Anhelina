using TMPro;
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    public TextMeshProUGUI missionText; // Tekstikentt‰, johon teht‰v‰t n‰ytet‰‰n

    // Korvaa koko teht‰v‰listan uudella tekstill‰
    public void SetMission(string text)
    {
        missionText.text = text;
    }

    // Lis‰‰ uuden teht‰v‰n listan loppuun uudelle riville
    public void AddMission(string mission)
    {
        missionText.text += "\nï " + mission;
    }
}