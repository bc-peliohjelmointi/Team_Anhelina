using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuLoader : MonoBehaviour
{
    public GameObject loadingPanel;
    public Slider loadingBar;

    public void PlayGame()
    {
        loadingPanel.SetActive(true); // show loading UI
        StartCoroutine(LoadSceneAsync()); // start async load
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);
        operation.allowSceneActivation = false; // wait for slider

        float displayedProgress = 0f;

        while (!operation.isDone)
        {
            // real progress 0 → 0.9
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // smooth slider movement
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime);
            loadingBar.value = displayedProgress;

            if (operation.progress >= 0.9f)
            {
                // finish last 10% smoothly
                displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.deltaTime);
                loadingBar.value = displayedProgress;

                // activate scene when slider hits 100%
                if (displayedProgress >= 1f)
                    operation.allowSceneActivation = true;
            }

            yield return null; // wait next frame
        }
    }
}