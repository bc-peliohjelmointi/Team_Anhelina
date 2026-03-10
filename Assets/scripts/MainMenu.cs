using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public GameObject mainPanel;      // Main buttons
    public GameObject playPanel;
    public GameObject optionsPanel;
    public GameObject storyPanel;       
    public GameObject confirmPanel;     
    public Text confirmText;
    public GameObject infoTextObject; 
    private Text uiText;

    private string sceneToLoad;

    void Start()
    {

        playPanel.SetActive(false);
        optionsPanel.SetActive(false);
        storyPanel.SetActive(false);
        mainPanel.SetActive(true);

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

    public void OpenOptions()   
    {
        mainPanel.SetActive(false);
        optionsPanel.SetActive(true);
        infoTextObject.SetActive(false);
    }

    public void BackFromOptions()   
    {
        optionsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

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

    public void OpenStory()
    {
        mainPanel.SetActive(false);
        storyPanel.SetActive(true);
    }

    public void BackFromStory()
    {
        storyPanel.SetActive(false);
        mainPanel.SetActive(true);
    }


    public void Chapter1Button()
    {
        confirmPanel.SetActive(true);
        storyPanel.SetActive(false);
        sceneToLoad = "sCENE 1";         
    }

    public void Chapter2Button()
    {
        confirmPanel.SetActive(true);
        storyPanel.SetActive(false);
        sceneToLoad = "Scene 1.5";
    }

    // Khi nhấn Chapter 3 button
    public void Chapter3Button()
    {
        confirmPanel.SetActive(true);
        storyPanel.SetActive(false);
        sceneToLoad = "scene 2";
    }

    // YES CONFIRL

    public void ConfirmLoadScene()
    {
        confirmPanel.SetActive(false);
        storyPanel.SetActive(false);
        mainPanel.SetActive(false);
        LevelManager.Instance.LoadScene(sceneToLoad, "CrossFade");
    }

    // NO confirm

    public void CancelLoadScene()
    {
        confirmPanel.SetActive(false);
        storyPanel.SetActive(true);
    }
}