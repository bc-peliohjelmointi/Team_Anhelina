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
    public KeyCode spaceExitKey = KeyCode.Space;

    [Header("References")]
    public PSScreen psScreen;
    public TVVideoPlayer tvVideoPlayer;
    public TVPowerEffect tvEffect;
    public TVButton tvButton;
    public EpisodeChecker episodeChecker;
    public PSInteraction psInteraction;

    [Header("UI")]
    public GameObject[] menuTextObjects;
    public GameObject errorTextObject;
    public float errorDisplayDuration = 5f;

    [Header("Timing")]
    public float exitDelayAfterSelect = 0.5f;

    private int currentIndex = 0;
    private bool navigationEnabled = false;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isShowingError = false;

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
    }

    void Update()
    {
        if (!navigationEnabled || isShowingError) return;

        if (Input.GetKeyDown(exitKey) || Input.GetKeyDown(spaceExitKey))
        {
            ExitNavigation();
            return;
        }

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

        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(false);
        }
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
        bool tvWasEverTurnedOn = false;

        if (tvButton != null)
        {
            tvWasEverTurnedOn = tvButton.WasEverTurnedOn();
        }

        if (!tvWasEverTurnedOn)
        {
            StartCoroutine(ShowTVNeverUsedError());
            return;
        }

        if (tvEffect == null || !tvEffect.IsOn())
        {
            StartCoroutine(ShowTVOffError());
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

        StartCoroutine(SelectAndExit(episodeNumber, isCorrectOrder));
    }

    System.Collections.IEnumerator ShowTVNeverUsedError()
    {
        isShowingError = true;

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

        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(true);
        }

        ShowMenuText();

        isShowingError = false;
    }

    System.Collections.IEnumerator ShowTVOffError()
    {
        isShowingError = true;

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

        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(true);
        }

        ShowMenuText();

        isShowingError = false;
    }

    System.Collections.IEnumerator SelectAndExit(int episodeNumber, bool isCorrectOrder)
    {
        if (tvVideoPlayer != null)
        {
            tvVideoPlayer.PlayEpisode(episodeNumber, isCorrectOrder);
        }

        yield return new WaitForSeconds(exitDelayAfterSelect);

        if (psInteraction != null)
        {
            yield return StartCoroutine(psInteraction.ExitPSView());
        }

        if (isCorrectOrder && psScreen != null)
        {
            psScreen.ShowCheckmark(currentIndex);
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