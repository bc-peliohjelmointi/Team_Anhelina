using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public GameObject mainPanel;      // Main buttons
    public GameObject playPanel;
    public GameObject infoTextObject; // текст слева
    private Text uiText;

    void Start()
    {

        playPanel.SetActive(false);

        if (infoTextObject != null)
        {
            infoTextObject.SetActive(false);
            uiText = infoTextObject.GetComponent<Text>();
        }
    }

    public void PlayGame()
    {
        mainPanel.SetActive(false);     // Hide main menu
        playPanel.SetActive(true);      // Show new options
        infoTextObject.SetActive(false);
    }

    // When pressing BACK inside PlayPanel
    public void BackToMain()
    {
        playPanel.SetActive(false);
        mainPanel.SetActive(true);

    }

    // When pressing START GAME
    public void StartNewGame()
    {
        LevelManager.Instance.LoadScene("sCENE 1", "CrossFade");
    }



    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowInfo(string text)
    {
        if (infoTextObject != null)
        {
            infoTextObject.SetActive(true);
            uiText.text = text;
        }
    }

    public void HideInfo()
    {
        if (infoTextObject != null)
        {
            infoTextObject.SetActive(false);
        }
    }
}