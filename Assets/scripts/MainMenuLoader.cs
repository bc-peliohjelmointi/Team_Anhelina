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
        loadingPanel.SetActive(true);
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(1);
        operation.allowSceneActivation = false;

        float displayedProgress = 0f;

        while (!operation.isDone)
        {
            // Real progress from 0 → 0.9
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);

            // Smoothly move the displayed slider
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime);
            loadingBar.value = displayedProgress;

            // Check if async load is ready
            if (operation.progress >= 0.9f)
            {
                // Smoothly finish the last 10%
                displayedProgress = Mathf.MoveTowards(displayedProgress, 1f, Time.deltaTime);
                loadingBar.value = displayedProgress;

                // Only activate scene when slider reaches 100%
                if (displayedProgress >= 1f)
                    operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }as
}
