using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject confirmPanel;
    [SerializeField] GameObject confirmChapterPanel;
    [SerializeField] GameObject chapter1Button;
    [SerializeField] GameObject chapter2Button;
    [SerializeField] GameObject chapter3Button;
    [SerializeField] GameObject optionsPanel;

    [SerializeField] GameObject storyPanel;

    private string sceneToLoad;

    private bool isPaused = false;


    void Start()
    {
        // Сlose confirmPanel
        confirmPanel.SetActive(false);
        confirmChapterPanel.SetActive(false);
        optionsPanel.SetActive(false);

        if (storyPanel != null)
            storyPanel.SetActive(false);
        int progress = PlayerPrefs.GetInt("StoryProgress", 1);
        chapter1Button.SetActive(true); // alwasy open


        chapter2Button.SetActive(progress >= 2);
        chapter3Button.SetActive(progress >= 3);


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            Pause();
        }
    }

    public void OpenOptions()
    {
        pauseMenu.SetActive(false);
        optionsPanel.SetActive(true);
        
    }

    public void BackFromOptions()
    {
        optionsPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    public void Home()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        
        pauseMenu.SetActive(false);

        
        confirmPanel.SetActive(true);
    }

    
    public void ConfirmQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); 
    }

    
    public void CancelQuit()
    {
        confirmPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void OpenStory()
    {
        pauseMenu.SetActive(false);
        storyPanel.SetActive(true);
    }

    public void BackFromStory()
    {
        storyPanel.SetActive(false);
        pauseMenu.SetActive(true);
    }

    public void Chapter1Button()
    {
        PlayerPrefs.SetInt("NewGame", 1);

        confirmChapterPanel.SetActive(true);
        storyPanel.SetActive(false);

        sceneToLoad = "sCENE 1";
    }

    public void Chapter2Button()
    {

        confirmChapterPanel.SetActive(true);
        storyPanel.SetActive(false);

        sceneToLoad = "Scene 1.5";
    }

    public void Chapter3Button()
    {
        confirmChapterPanel.SetActive(true);
        storyPanel.SetActive(false);

        sceneToLoad = "scene 2";
    }

    public void ConfirmLoadScene()
    {
        Time.timeScale = 1f;

        confirmChapterPanel.SetActive(false);
        pauseMenu.SetActive(false);

        int isNewGame = PlayerPrefs.GetInt("NewGame", 0);

        if (isNewGame == 1)
        {
            Debug.Log("RESET SAVE FOR NEW GAME");

            PlayerPrefs.DeleteKey("PlayerX");
            PlayerPrefs.DeleteKey("PlayerY");
            PlayerPrefs.DeleteKey("PlayerZ");
            PlayerPrefs.DeleteKey("CurrentScene");

            PlayerPrefs.Save();
        }

        LevelManager.Instance.LoadScene(sceneToLoad, "CrossFade");
    }

    public void CancelLoadScene()
    {
        confirmChapterPanel.SetActive(false);
        storyPanel.SetActive(true);
    }
}
