using UnityEngine;

public class PSMenuNavigation : MonoBehaviour
{
    [Header("Menu Items")]
    public Transform[] menuPositions = new Transform[3];
    public Transform selectionRectangle;

    [Header("Navigation Settings")]
    public float moveSpeed = 10f;
    public KeyCode selectKey = KeyCode.Return;

    [Header("References")]
    public PSScreen psScreen;
    public TVVideoPlayer tvVideoPlayer;
    public TVPowerEffect tvEffect;
    public EpisodeChecker episodeChecker;

    [Header("UI")]
    public PSMenuObject errorDisplay;

    private int currentIndex = 0;
    private bool navigationEnabled = false;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        if (selectionRectangle != null)
        {
            selectionRectangle.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!navigationEnabled) return;

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
            ShowTVOffError();
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

        if (tvVideoPlayer != null)
        {
            tvVideoPlayer.PlayEpisode(episodeNumber, isCorrectOrder);
        }

        if (isCorrectOrder && psScreen != null)
        {
            psScreen.ShowCheckmark(currentIndex);
        }
    }

    void ShowTVOffError()
    {
        if (errorDisplay != null)
        {
            StartCoroutine(BlinkError());
        }
    }

    System.Collections.IEnumerator BlinkError()
    {
        for (int i = 0; i < 6; i++)
        {
            if (errorDisplay != null)
            {
                errorDisplay.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(0.3f);

            if (errorDisplay != null)
            {
                errorDisplay.gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }
}