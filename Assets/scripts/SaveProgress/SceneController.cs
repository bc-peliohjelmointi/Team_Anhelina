using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController instance; // singleton instance
    public bool isNewGame = true; // flag for new game

    private void Awake()
    {
        if (instance == null)
        {
            instance = this; // assign instance
            DontDestroyOnLoad(gameObject); // persist across scenes
        }
        else
        {
            Destroy(gameObject); // remove duplicate
        }
    }

    public void NextLevel()
    {
        // load next scene by build index
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void LoadScene(string sceneName)
    {
        // load scene by name 
        SceneManager.LoadSceneAsync(sceneName);
    }
}