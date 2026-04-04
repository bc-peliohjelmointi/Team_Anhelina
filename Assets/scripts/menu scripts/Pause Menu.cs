using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu; // main pause UI
    [SerializeField] GameObject confirmPanel; // quit confirm
    [SerializeField] GameObject confirmChapterPanel; // chapter confirm
    [SerializeField] GameObject chapter1Button;
    [SerializeField] GameObject chapter2Button;
    [SerializeField] GameObject chapter3Button;
    [SerializeField] GameObject optionsPanel; // options UI

    [SerializeField] GameObject storyPanel; // story UI

    private string sceneToLoad; // selected scene

    private bool isPaused = false; // pause state

    void Start()
    {
        // hide panels at start
        confirmPanel.SetActive(false);
        confirmChapterPanel.SetActive(false);
        optionsPanel.SetActive(false);

        if (storyPanel != null)
            storyPanel.SetActive(false);

        // load story progress
        int progress = PlayerPrefs.GetInt("StoryProgress", 1);

        chapter1Button.SetActive(true); // always unlocked
        chapter2Button.SetActive(progress >= 2); // unlock if progress
        chapter3Button.SetActive(progress >= 3);
    }

    void Update()
    {
        // press ESC to pause
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            Pause();
        }
    }

    public void OpenOptions()
    {
        pauseMenu.SetActive(false); // hide pause
        optionsPanel.SetActive(true); // show options
    }

    public void BackFromOptions()
    {
        optionsPanel.SetActive(false);
        pauseMenu.SetActive(true); // back to pause
    }

    public void Pause()
    {
        pauseMenu.SetActive(true); // show menu
        Time.timeScale = 0f; // freeze game
        isPaused = true;

        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Cursor.visible = true;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false); // hide menu
        Time.timeScale = 1f; // resume game
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked; // lock cursor
        Cursor.visible = false;
    }

    public void Home()
    {
        Time.timeScale = 1f; // reset time
        SceneManager.LoadScene(0); // load main menu
    }

    public void Quit()
    {
        pauseMenu.SetActive(false); // hide pause
        confirmPanel.SetActive(true); // show confirm quit
    }

    public void ConfirmQuit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0); // go to menu
    }

    public void CancelQuit()
    {
        confirmPanel.SetActive(false);
        pauseMenu.SetActive(true); // return to pause
    }

    public void OpenStory()
    {
        pauseMenu.SetActive(false);
        storyPanel.SetActive(true); // open story
    }

    public void BackFromStory()
    {
        storyPanel.SetActive(false);
        pauseMenu.SetActive(true); // back to pause
    }

    public void Chapter1Button()
    {
        PlayerPrefs.SetInt("NewGame", 1); // mark new game
        PlayerPrefs.Save();

        confirmChapterPanel.SetActive(true);
        storyPanel.SetActive(false);

        sceneToLoad = "sCENE 1"; // set scene
    }

    public void Chapter2Button()
    {
        confirmChapterPanel.SetActive(true);
        storyPanel.SetActive(false);

        sceneToLoad = "Scene 1.5"; // set scene
    }

    public void Chapter3Button()
    {
        confirmChapterPanel.SetActive(true);
        storyPanel.SetActive(false);

        sceneToLoad = "scene 2"; // set scene
    }

    public void ConfirmLoadScene()
    {
        Time.timeScale = 1f; // resume time

        confirmChapterPanel.SetActive(false);
        pauseMenu.SetActive(false);

        int isNewGame = PlayerPrefs.GetInt("NewGame", 0);

        if (isNewGame == 1)
        {
            Debug.Log("RESET SAVE FOR NEW GAME");

            // clear saved player data
            PlayerPrefs.DeleteKey("PlayerX");
            PlayerPrefs.DeleteKey("PlayerY");
            PlayerPrefs.DeleteKey("PlayerZ");
            PlayerPrefs.DeleteKey("CurrentScene");

            PlayerPrefs.Save();
        }

        // load selected scene with transition
        LevelManager.Instance.LoadScene(sceneToLoad, "CrossFade");
    }

    public void CancelLoadScene()
    {
        confirmChapterPanel.SetActive(false);
        storyPanel.SetActive(true); // back to story
    }
}