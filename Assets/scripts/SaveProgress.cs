using UnityEngine;

public class SaveProgress : MonoBehaviour
{
    public int chapterNumber;

    void Start()
    {
        int progress = PlayerPrefs.GetInt("StoryProgress", 1);

        if (chapterNumber > progress)
        {
            PlayerPrefs.SetInt("StoryProgress", chapterNumber);
            PlayerPrefs.Save();
        }
    }
}