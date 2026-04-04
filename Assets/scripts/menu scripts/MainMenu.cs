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
    private string continueSceneToLoad;

    void Start()
    {
        // init panels
        playPanel.SetActive(false);
        optionsPanel.SetActive(false);
        storyPanel.SetActive(false);
        mainPanel.SetActive(true);

        if (infoTextObject != null)
        {
            infoTextObject.SetActive(false); // hide info at start
            uiText = infoTextObject.GetComponent<Text>(); // grab text comp
        }
    }

    public void PlayGame() // this is actually continue button
    {
        mainPanel.SetActive(false);     // hide main menu
        playPanel.SetActive(true);      // show play options
        infoTextObject.SetActive(false);

        // check does PlayerPrefs save data
        if (PlayerPrefs.HasKey("CurrentScene"))
            continueSceneToLoad = PlayerPrefs.GetString("CurrentScene"); // load saved scene
        else
            continueSceneToLoad = "sCENE 1"; ;   // default first scene
    }

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

    public void StartNewGame()
    {
        playPanel.SetActive(false);
        storyPanel.SetActive(false);
        mainPanel.SetActive(false);

        if (!string.IsNullOrEmpty(continueSceneToLoad))
        {
            PlayerPrefs.SetInt("NewGame", 0);
            PlayerPrefs.Save();

            LevelManager.Instance.LoadScene(continueSceneToLoad, "CrossFade"); // load continue
            continueSceneToLoad = null; // reset after load
        }
        else
        {
            PlayerPrefs.SetInt("NewGame", 1);
            PlayerPrefs.Save();
            LevelManager.Instance.LoadScene(sceneToLoad, "CrossFade"); // load new chapter
        }
    }

    public void QuitGame()
    {
        Application.Quit(); // exit app
    }

    public void ShowInfo(string text)
    {
        if (infoTextObject != null)
        {
            infoTextObject.SetActive(true); // show info
            uiText.text = text;
        }
    }

    public void HideInfo()
    {
        if (infoTextObject != null)
        {
            infoTextObject.SetActive(false); // hide info
        }
    }

    public void OpenStory()
    {
        mainPanel.SetActive(false);
        storyPanel.SetActive(true); // show story
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
        sceneToLoad = "sCENE 1";         // set scene
    }

    public void Chapter2Button()
    {
        confirmPanel.SetActive(true);
        storyPanel.SetActive(false);
        sceneToLoad = "Scene 1.5"; // set scene
    }

    public void Chapter3Button()
    {
        confirmPanel.SetActive(true);
        storyPanel.SetActive(false);
        sceneToLoad = "scene 2"; // set scene
    }

    public void ConfirmLoadScene()
    {
        confirmPanel.SetActive(false);
        storyPanel.SetActive(false);
        mainPanel.SetActive(false);
        LevelManager.Instance.LoadScene(sceneToLoad, "CrossFade"); // load confirmed
    }

    public void CancelLoadScene()
    {
        confirmPanel.SetActive(false);
        storyPanel.SetActive(true); // cancel, back to story
    }

}