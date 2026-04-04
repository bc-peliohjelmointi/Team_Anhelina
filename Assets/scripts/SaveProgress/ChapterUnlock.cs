using UnityEngine;

public class ChapterUnlock : MonoBehaviour
{
    public GameObject chapter2Button;
    public GameObject chapter3Button;

    void Start()
    {
        int progress = PlayerPrefs.GetInt("StoryProgress", 1); // get progress default 1

        chapter2Button.SetActive(progress >= 2); // unlock chapter 2
        chapter3Button.SetActive(progress >= 3); // unlock chapter 3
    }
}