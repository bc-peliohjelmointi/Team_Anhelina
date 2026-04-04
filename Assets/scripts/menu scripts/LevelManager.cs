using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance; 

    public Slider progressBar; // UI loading bar
    public GameObject transitionsContainer; // parent of all transitions
    private SceneTransition[] transitions; // list of transitions

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; // assign singleton
            DontDestroyOnLoad(gameObject); // keep between scenes
        }
        else
        {
            Destroy(gameObject); // destroy duplicate
        }
    }

    private void Start()
    {
        // collect all transition scripts from children
        transitions = transitionsContainer.GetComponentsInChildren<SceneTransition>();
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        // start async loading process
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        // find transition by name
        SceneTransition transition = transitions.First(t => t.name == transitionName);

        // start loading scene in background
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false; // delay scene switch

        // play transition IN 
        yield return transition.AnimateTransitionIn();

        progressBar.gameObject.SetActive(true); // show loading bar

        // update loading progress until almost done
        do
        {
            progressBar.value = scene.progress; // set slider value
            yield return null; // wait next frame
        } while (scene.progress < 0.9f); // Unity stops at 0.9

        scene.allowSceneActivation = true; // now switch scene
        progressBar.gameObject.SetActive(false); // hide loading bar

        // play transition OUT
        yield return StartCoroutine(transition.AnimateTransitionOut());
    }
}