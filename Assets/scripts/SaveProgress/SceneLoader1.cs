using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader1 : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // load scene by name
    }

    public void LoadSceneByIndex(int index)
    {
        SceneManager.LoadScene(index); // load scene by build index
    }

    public void QuitGame()
    {
        Application.Quit(); // quit application
        Debug.Log("Game Quit"); // debug message
    }
}