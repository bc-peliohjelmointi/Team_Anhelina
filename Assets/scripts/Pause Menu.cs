using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject confirmPanel;
    private bool isPaused = false;


    void Start()
    {
        // Сразу скрываем confirmPanel
        confirmPanel.SetActive(false);
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
}
