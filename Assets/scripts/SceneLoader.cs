using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSceneByName(string vanya)
    {
        SceneManager.LoadScene(vanya);
    }
}