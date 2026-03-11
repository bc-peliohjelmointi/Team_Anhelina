using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject confirmPanel;
    [SerializeField] GameObject confirmChapterPanel;

    [SerializeField] GameObject storyPanel;

    private string sceneToLoad;

    private bool isPaused = false;


    void Start()
    {
        // Сразу скрываем confirmPanel
        confirmPanel.SetActive(false);
        confirmChapterPanel.SetActive(false);

        if (storyPanel != null)
            storyPanel.SetActive(false);


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            Pause();
        }
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

    public void Chapter3Button()
    {
        confirmPanel.SetActive(true);
        storyPanel.SetActive(false);

        sceneToLoad = "scene 2";
    }

    public void ConfirmLoadScene()
    {
        Time.timeScale = 1f;

        confirmPanel.SetActive(false);
        pauseMenu.SetActive(false);

        LevelManager.Instance.LoadScene(sceneToLoad, "CrossFade");
    }

    public void CancelLoadScene()
    {
        confirmPanel.SetActive(false);
        storyPanel.SetActive(true);
    }
}
