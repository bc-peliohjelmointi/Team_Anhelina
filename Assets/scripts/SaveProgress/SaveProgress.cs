using UnityEngine;

public class SaveProgress : MonoBehaviour
{
    public int chapterNumber; // current chapter index

    void Start()
    {
        int progress = PlayerPrefs.GetInt("StoryProgress", 1); // load saved progress

        // update progress only if this chapter is higher
        if (chapterNumber > progress)
        {
            PlayerPrefs.SetInt("StoryProgress", chapterNumber); // save new progress
            PlayerPrefs.Save();
        }
    }
}