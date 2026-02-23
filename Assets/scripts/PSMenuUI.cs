using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PSMenuUI : MonoBehaviour
{
    public Canvas menuCanvas;
    public Text[] menuOptions;
    public Image selectionHighlight;
    public Image[] checkmarks;
    public Text errorText;
    public Color normalColor = Color.green;
    public Color selectedColor = Color.white;
    public float blinkSpeed = 0.5f;

    public TVPowerEffect tvEffect;
    public PSButton psButton;
    public EpisodeChecker episodeChecker;
    public PSInteraction psInteraction;
    public TVVideoPlayer videoPlayer;

    private int selectedIndex = 0;
    private int maxIndex = 2;
    private bool canNavigate = true;
    private bool[] episodesCompleted = new bool[3];

    void Start()
    {
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }

        for (int i = 0; i < checkmarks.Length; i++)
        {
            if (checkmarks[i] != null)
            {
                checkmarks[i].enabled = false;
            }
        }

        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!menuCanvas.gameObject.activeSelf || !canNavigate)
            return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = maxIndex;
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex++;
            if (selectedIndex > maxIndex) selectedIndex = 0;
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectOption();
        }
    }

    public void ShowMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(true);
        }
        UpdateSelection();
    }

    public void HideMenu()
    {
        if (menuCanvas != null)
        {
            menuCanvas.gameObject.SetActive(false);
        }

        if (errorText != null)
        {
            errorText.gameObject.SetActive(false);
        }
    }

    void UpdateSelection()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
            if (menuOptions[i] != null)
            {
                menuOptions[i].color = (i == selectedIndex) ? selectedColor : normalColor;
            }
        }

        if (selectionHighlight != null && selectedIndex < menuOptions.Length)
        {
            selectionHighlight.transform.position = menuOptions[selectedIndex].transform.position;
        }
    }

    void SelectOption()
    {
        StartCoroutine(ProcessSelection());
    }

    IEnumerator ProcessSelection()
    {
        canNavigate = false;

        if (tvEffect == null || !tvEffect.IsOn())
        {
            yield return StartCoroutine(ShowError("TURN ON TV"));
            canNavigate = true;
            yield break;
        }

        bool isCorrect = false;

        if (selectedIndex == 0)
        {
            isCorrect = episodeChecker.IsEpisode1Correct();
        }
        else if (selectedIndex == 1)
        {
            isCorrect = episodeChecker.IsEpisode2Correct();
        }
        else if (selectedIndex == 2)
        {
            isCorrect = episodeChecker.IsEpisode3Correct();
        }

        if (psInteraction != null)
        {
            psInteraction.ExitPSView();
        }

        yield return new WaitForSeconds(0.5f);

        if (videoPlayer != null)
        {
            videoPlayer.PlayEpisode(selectedIndex + 1, isCorrect);
        }

        if (isCorrect)
        {
            episodesCompleted[selectedIndex] = true;
            if (checkmarks[selectedIndex] != null)
            {
                checkmarks[selectedIndex].enabled = true;
            }
        }

        canNavigate = true;
    }

    IEnumerator ShowError(string message)
    {
        if (errorText != null)
        {
            errorText.text = message;
            errorText.gameObject.SetActive(true);

            for (int i = 0; i < 6; i++)
            {
                errorText.color = Color.red;
                yield return new WaitForSeconds(blinkSpeed);
                errorText.color = Color.clear;
                yield return new WaitForSeconds(blinkSpeed);
            }

            errorText.gameObject.SetActive(false);
        }
    }
}