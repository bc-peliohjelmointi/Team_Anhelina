using UnityEngine;
using System.Collections;

public class PSMenuNavigation : MonoBehaviour
{
    [Header("Menu Items")]
    public Transform[] menuPositions = new Transform[3];
    public Transform selectionRectangle;

    [Header("Navigation Settings")]
    public float moveSpeed = 10f;
    public KeyCode selectKey = KeyCode.Return;
    public KeyCode exitKey = KeyCode.E;

    [Header("References")]
    public PSScreen psScreen;
    public TVVideoPlayer tvVideoPlayer;
    public TVPowerEffect tvEffect;
    public EpisodeChecker episodeChecker;
    public PSInteraction psInteraction;

    [Header("UI")]
    public GameObject[] menuTextObjects;
    public GameObject errorTextObject;
    public float errorDisplayDuration = 5f;

    [Header("Checkmarks")]
    public GameObject[] checkmarkObjects;

    [Header("Timing")]
    public float delayBeforeCheckmark = 1f;

    private int currentIndex = 0;
    private bool navigationEnabled = false;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isShowingError = false;
    private bool allowExit = true;

    void Start()
    {
        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(false);
        }

        if (errorTextObject != null)
        {
            errorTextObject.SetActive(false);
        }

        HideAllCheckmarks();
    }

    void Update()
    {
        if (!navigationEnabled) return;

        if (allowExit && Input.GetKeyDown(exitKey))
        {
            ExitNavigation();
            return;
        }

        if (isShowingError) return;

        if (!isMoving)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                MoveUp();
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                MoveDown();
            }
        }

        if (Input.GetKeyDown(selectKey))
        {
            SelectCurrentOption();
        }

        if (selectionRectangle != null && isMoving)
        {
            selectionRectangle.position = Vector3.MoveTowards(
                selectionRectangle.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );

            if (Vector3.Distance(selectionRectangle.position, targetPosition) < 0.001f)
            {
                selectionRectangle.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void EnableNavigation()
    {
        navigationEnabled = true;
        currentIndex = 0;
        isMoving = false;
        allowExit = true;

        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(true);

            if (menuPositions.Length > 0 && menuPositions[0] != null)
            {
                targetPosition = menuPositions[0].position;
                selectionRectangle.position = targetPosition;
            }
        }

        ShowMenuText();
        UpdateSelection();
    }

    public void DisableNavigation()
    {
        navigationEnabled = false;
        isMoving = false;
        allowExit = true;

        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(false);
        }

        HideMenuText();
    }

    void ExitNavigation()
    {
        if (psInteraction != null)
        {
            StartCoroutine(psInteraction.ExitPSView());
        }
    }

    void MoveUp()
    {
        int newIndex = currentIndex - 1;

        if (newIndex >= 0)
        {
            currentIndex = newIndex;
            UpdateSelection();
            isMoving = true;
        }
    }

    void MoveDown()
    {
        int newIndex = currentIndex + 1;

        if (newIndex < menuPositions.Length)
        {
            currentIndex = newIndex;
            UpdateSelection();
            isMoving = true;
        }
    }

    void UpdateSelection()
    {
        if (menuPositions.Length > 0 && currentIndex >= 0 && currentIndex < menuPositions.Length && menuPositions[currentIndex] != null)
        {
            targetPosition = menuPositions[currentIndex].position;
        }

        if (psScreen != null)
        {
            psScreen.UpdateSelection(currentIndex);
        }
    }

    void SelectCurrentOption()
    {
        if (tvEffect == null || !tvEffect.IsOn())
        {
            StartCoroutine(ShowTVOffErrorAndExit());
            return;
        }

        int episodeNumber = currentIndex + 1;

        bool isCorrectOrder = false;

        if (episodeChecker != null)
        {
            if (episodeNumber == 1)
            {
                isCorrectOrder = episodeChecker.IsEpisode1Correct();
            }
            else if (episodeNumber == 2)
            {
                isCorrectOrder = episodeChecker.IsEpisode2Correct();
            }
            else if (episodeNumber == 3)
            {
                isCorrectOrder = episodeChecker.IsEpisode3Correct();
            }
        }

        StartCoroutine(SelectAndExit(episodeNumber, isCorrectOrder, currentIndex));
    }

    System.Collections.IEnumerator ShowTVOffErrorAndExit()
    {
        isShowingError = true;
        allowExit = false;

        HideMenuText();

        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(false);
        }

        if (errorTextObject != null)
        {
            errorTextObject.SetActive(true);
        }

        yield return new WaitForSeconds(errorDisplayDuration);

        if (errorTextObject != null)
        {
            errorTextObject.SetActive(false);
        }

        isShowingError = false;
        allowExit = true;

        if (psInteraction != null)
        {
            yield return StartCoroutine(psInteraction.ExitPSView());
        }
    }

    System.Collections.IEnumerator SelectAndExit(int episodeNumber, bool isCorrectOrder, int checkmarkIndex)
    {
        allowExit = false;

        if (tvVideoPlayer != null)
        {
            tvVideoPlayer.PlayEpisode(episodeNumber, isCorrectOrder);
        }

        yield return new WaitForSeconds(0.5f);

        if (psInteraction != null)
        {
            yield return StartCoroutine(psInteraction.ExitPSView());
        }

        if (isCorrectOrder)
        {
            float videoDuration = CalculateVideoDuration(episodeNumber, isCorrectOrder);
            yield return new WaitForSeconds(videoDuration + delayBeforeCheckmark);
            ShowCheckmark(checkmarkIndex);
        }

        allowExit = true;
    }

    float CalculateVideoDuration(int episodeNumber, bool isCorrect)
    {
        if (tvVideoPlayer == null) return 0f;

        float duration = tvVideoPlayer.introDisplayDuration;

        if (isCorrect)
        {
            int quadCount = 0;
            if (episodeNumber == 1 && tvVideoPlayer.episode1CorrectQuads != null)
                quadCount = tvVideoPlayer.episode1CorrectQuads.Length;
            else if (episodeNumber == 2 && tvVideoPlayer.episode2CorrectQuads != null)
                quadCount = tvVideoPlayer.episode2CorrectQuads.Length;
            else if (episodeNumber == 3 && tvVideoPlayer.episode3CorrectQuads != null)
                quadCount = tvVideoPlayer.episode3CorrectQuads.Length;

            duration += quadCount * tvVideoPlayer.correctQuadDuration;
            duration += 1f;
        }
        else
        {
            duration += tvVideoPlayer.wrongCartoonDuration;
            duration += tvVideoPlayer.errorDisplayDuration;
        }

        return duration;
    }

    void ShowCheckmark(int index)
    {
        if (checkmarkObjects != null && index >= 0 && index < checkmarkObjects.Length)
        {
            if (checkmarkObjects[index] != null)
            {
                checkmarkObjects[index].SetActive(true);
            }
        }
    }

    void HideAllCheckmarks()
    {
        if (checkmarkObjects != null)
        {
            foreach (GameObject checkmark in checkmarkObjects)
            {
                if (checkmark != null)
                {
                    checkmark.SetActive(false);
                }
            }
        }
    }

    void HideMenuText()
    {
        if (menuTextObjects != null)
        {
            foreach (GameObject textObj in menuTextObjects)
            {
                if (textObj != null)
                {
                    textObj.SetActive(false);
                }
            }
        }
    }

    void ShowMenuText()
    {
        if (menuTextObjects != null)
        {
            foreach (GameObject textObj in menuTextObjects)
            {
                if (textObj != null)
                {
                    textObj.SetActive(true);
                }
            }
        }
    }
}