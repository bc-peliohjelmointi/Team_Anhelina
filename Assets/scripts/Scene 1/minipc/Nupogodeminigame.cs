using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// ============================================================
// NuPogodeMiniGame.cs  —  SIMPLE STANDALONE VERSION
// ============================================================
// No DrawerSystem, no CheckpointManager, no GameFlowManager
// This game exists on its own and affects nothing else
//
// RULES:
//   catch egg       = +1 score
//   miss egg        = -1 life
//   catch rabbit    = SCREAMER + -1 life
//   miss rabbit     = nothing (it just disappears)
//   lose all lives  = game over -> leaderboard -> desktop
//   reach scoreToWin -> pause "Continue? Yes / No"
//     Yes -> keep playing until lives run out -> leaderboard
//     No  -> return to PSDesktop
//
// CONTROLS: A=top-left  Z=bottom-left  D=top-right  X=bottom-right
//
// CALLED BY: PSDesktop.miniGame.StartGame()
// RETURNS TO: PSDesktop.Instance.ReturnToDesktop()
// ============================================================

public class NuPogodeMiniGame : MonoBehaviour
{
    // ---- score ----
    [Header("Score")]
    // score needed to trigger the "Continue?" pause - set in inspector
    public int scoreToWin = 100;

    // ---- panels ----
    [Header("Panels")]
    // main game panel shown while playing
    public GameObject gameplayPanel;
    // "Continue? Yes / No" shown when scoreToWin is reached
    public GameObject continuePanel;
    // top scores panel shown after game over
    public GameObject leaderboardPanel;

    // ---- gameplay UI ----
    [Header("Gameplay UI")]
    // score display "0000" top center
    public Text scoreText;
    // 3 rabbit-head Images as life icons
    public Image lifeIcon1, lifeIcon2, lifeIcon3;
    // sprite for active life
    public Sprite lifeActiveSprite;
    // sprite for lost life (dark or crossed)
    public Sprite lifeLostSprite;
    // full screen scary robot rabbit image - shown on screamer hit
    public Image screamerImage;
    // dark semi-transparent overlay behind screamer
    public Image darkFlashOverlay;

    // ---- leaderboard ----
    [Header("Leaderboard")]
    public Text leaderboardText;
    // close button returns to desktop
    public Button leaderboardCloseButton;

    // ---- continue panel ----
    [Header("Continue Panel")]
    public Button continueYesButton;
    public Button continueNoButton;

    // ---- spawn points ----
    [Header("Spawn Points (4 columns, top and bottom)")]
    // empty RectTransforms at top edge of each column
    public RectTransform spawnTop1, spawnTop2, spawnTop3, spawnTop4;
    // empty RectTransforms at bottom edge of each column
    public RectTransform spawnBot1, spawnBot2, spawnBot3, spawnBot4;
    // prefab: Image with pixel egg sprite
    public GameObject eggPrefab;
    // prefab: Image with scary robot rabbit head sprite
    public GameObject rabbitHeadPrefab;
    // parent for all falling objects
    public RectTransform fallingObjectsParent;

    // ---- wolf ----
    [Header("Wolf Catch Zones")]
    // empty RectTransforms at the 4 catch positions
    public RectTransform catchZoneTopLeft;
    public RectTransform catchZoneTopRight;
    public RectTransform catchZoneBotLeft;
    public RectTransform catchZoneBotRight;
    // 4 wolf images - only the active one is visible
    public Image wolfTopLeft, wolfTopRight, wolfBotLeft, wolfBotRight;
    // pixel radius to count as a successful catch
    public float catchRadius = 60f;

    // ---- difficulty ----
    [Header("Difficulty")]
    public float startFallSpeed = 180f;
    // extra speed per 10 points scored
    public float speedIncreasePerTen = 20f;
    public float maxFallSpeed = 600f;
    public float startSpawnInterval = 1.4f;
    public float minSpawnInterval = 0.4f;
    // 0-1 chance a spawned object is a rabbit head
    public float baseRabbitChance = 0.15f;

    // ---- audio ----
    [Header("Audio")]
    public AudioSource audioSource;
    // loud jump scare when rabbit head is caught
    public AudioClip screamerSound;
    // short pop when egg is caught
    public AudioClip catchEggSound;
    // thud when egg is missed
    public AudioClip dropEggSound;
    // jingle on game over
    public AudioClip gameOverSound;
    // fanfare when scoreToWin is reached
    public AudioClip winSound;
    public float soundVolume = 0.8f;

    // ---- private ----
    private int currentScore = 0;
    private int currentLives = 3;
    private bool isPlaying = false;
    private bool isPaused = false;
    private float currentFallSpeed;
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;
    private List<int> leaderboardScores = new List<int>();
    // 0=TopLeft 1=TopRight 2=BotLeft 3=BotRight
    private int activeWolfPos = 2;

    private class FallingObj
    {
        public RectTransform rect;
        public int column;  // 0-3 left to right
        public bool fromTop; // true=falls down, false=rises up
        public bool isRabbit;
        public bool done;
    }
    private List<FallingObj> activeObjects = new List<FallingObj>();

    // ============================================================
    // ENTRY POINT — called by PSDesktop after PLAY is clicked
    // ============================================================
    public void StartGame()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        InitGame();
    }

    void InitGame()
    {
        currentScore = 0;
        currentLives = 3;
        currentFallSpeed = startFallSpeed;
        currentSpawnInterval = startSpawnInterval;
        isPaused = false;
        isPlaying = true;
        activeWolfPos = 2;

        ClearAllObjects();
        RefreshScoreUI();
        RefreshLivesUI();
        SetWolfPosition(activeWolfPos);

        if (screamerImage != null) screamerImage.gameObject.SetActive(false);
        if (darkFlashOverlay != null) darkFlashOverlay.gameObject.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
        if (continuePanel != null) continuePanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    // ============================================================
    // UNITY LOOP
    // ============================================================
    void Update()
    {
        if (!isPlaying || isPaused) return;
        HandleInput();
        TickObjects();
        CheckCatches();
        UpdateDifficulty();
    }

    void HandleInput()
    {
        // latin keys: A / Z / D / X
        if (Input.GetKeyDown(KeyCode.A)) SetWolfPosition(0); // top-left
        if (Input.GetKeyDown(KeyCode.Z)) SetWolfPosition(2); // bottom-left
        if (Input.GetKeyDown(KeyCode.D)) SetWolfPosition(1); // top-right
        if (Input.GetKeyDown(KeyCode.X)) SetWolfPosition(3); // bottom-right
    }

    void SetWolfPosition(int pos)
    {
        activeWolfPos = pos;
        if (wolfTopLeft != null) wolfTopLeft.gameObject.SetActive(pos == 0);
        if (wolfTopRight != null) wolfTopRight.gameObject.SetActive(pos == 1);
        if (wolfBotLeft != null) wolfBotLeft.gameObject.SetActive(pos == 2);
        if (wolfBotRight != null) wolfBotRight.gameObject.SetActive(pos == 3);
    }

    void UpdateDifficulty()
    {
        float boost = (currentScore / 10) * speedIncreasePerTen;
        currentFallSpeed = Mathf.Min(startFallSpeed + boost, maxFallSpeed);

        float reduction = (currentScore / 10) * 0.08f;
        currentSpawnInterval = Mathf.Max(startSpawnInterval - reduction, minSpawnInterval);
    }

    // ============================================================
    // SPAWNING
    // ============================================================
    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(1f);
        while (isPlaying)
        {
            if (!isPaused) SpawnObject();
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    void SpawnObject()
    {
        int col = Random.Range(0, 4);
        bool fromTop = Random.value > 0.5f;
        bool isRabbit = Random.value < (baseRabbitChance + currentScore * 0.001f);

        GameObject prefab = isRabbit ? rabbitHeadPrefab : eggPrefab;
        if (prefab == null || fallingObjectsParent == null) return;

        GameObject go = Instantiate(prefab, fallingObjectsParent);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) { Destroy(go); return; }

        RectTransform sp = GetSpawnPoint(col, fromTop);
        if (sp != null) rt.anchoredPosition = sp.anchoredPosition;

        activeObjects.Add(new FallingObj
        {
            rect = rt,
            column = col,
            fromTop = fromTop,
            isRabbit = isRabbit,
            done = false
        });
    }

    RectTransform GetSpawnPoint(int col, bool top)
    {
        if (top)
        {
            switch (col)
            {
                case 0: return spawnTop1;
                case 1: return spawnTop2;
                case 2: return spawnTop3;
                case 3: return spawnTop4;
            }
        }
        else
        {
            switch (col)
            {
                case 0: return spawnBot1;
                case 1: return spawnBot2;
                case 2: return spawnBot3;
                case 3: return spawnBot4;
            }
        }
        return null;
    }

    // ============================================================
    // OBJECT MOVEMENT
    // ============================================================
    void TickObjects()
    {
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            FallingObj fo = activeObjects[i];
            if (fo.done || fo.rect == null) { activeObjects.RemoveAt(i); continue; }

            Vector2 p = fo.rect.anchoredPosition;
            p.y += (fo.fromTop ? -1f : 1f) * currentFallSpeed * Time.deltaTime;
            fo.rect.anchoredPosition = p;

            bool missed = fo.fromTop ? (p.y < -500f) : (p.y > 500f);
            if (missed)
            {
                fo.done = true;
                Destroy(fo.rect.gameObject);
                activeObjects.RemoveAt(i);

                if (!fo.isRabbit)
                {
                    // missed egg = -1 life, no screamer
                    if (dropEggSound != null)
                        audioSource.PlayOneShot(dropEggSound, soundVolume);
                    LoseLife();
                }
                // missed rabbit = nothing, just disappears
            }
        }
    }

    // ============================================================
    // CATCH DETECTION
    // ============================================================
    void CheckCatches()
    {
        RectTransform activeZone = null;
        int colMin = 0;
        bool activeTop = false;

        switch (activeWolfPos)
        {
            case 0: activeZone = catchZoneTopLeft; colMin = 0; activeTop = true; break;
            case 1: activeZone = catchZoneTopRight; colMin = 2; activeTop = true; break;
            case 2: activeZone = catchZoneBotLeft; colMin = 0; activeTop = false; break;
            case 3: activeZone = catchZoneBotRight; colMin = 2; activeTop = false; break;
        }
        if (activeZone == null) return;

        int colMax = colMin + 1;
        Vector2 zonePos = activeZone.anchoredPosition;

        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            FallingObj fo = activeObjects[i];
            if (fo.done || fo.rect == null) continue;
            if (fo.column < colMin || fo.column > colMax) continue;
            if (fo.fromTop != activeTop) continue;

            if (Vector2.Distance(fo.rect.anchoredPosition, zonePos) < catchRadius)
            {
                fo.done = true;
                Destroy(fo.rect.gameObject);
                activeObjects.RemoveAt(i);

                if (fo.isRabbit) StartCoroutine(CaughtRabbit());
                else CaughtEgg();
            }
        }
    }

    // ============================================================
    // CATCH RESULTS
    // ============================================================
    void CaughtEgg()
    {
        // caught egg = +1 score
        if (catchEggSound != null) audioSource.PlayOneShot(catchEggSound, soundVolume);
        currentScore++;
        RefreshScoreUI();

        // reached score target -> pause and ask continue
        if (currentScore >= scoreToWin)
            StartCoroutine(WinPauseSequence());
    }

    IEnumerator CaughtRabbit()
    {
        // caught rabbit = screamer + -1 life
        if (screamerSound != null) audioSource.PlayOneShot(screamerSound, 1f);
        if (screamerImage != null) screamerImage.gameObject.SetActive(true);
        if (darkFlashOverlay != null)
        {
            darkFlashOverlay.gameObject.SetActive(true);
            darkFlashOverlay.color = new Color(0f, 0f, 0f, 0.55f);
        }
        yield return new WaitForSeconds(0.7f);
        if (screamerImage != null) screamerImage.gameObject.SetActive(false);
        if (darkFlashOverlay != null) darkFlashOverlay.gameObject.SetActive(false);
        LoseLife();
    }

    void LoseLife()
    {
        currentLives--;
        RefreshLivesUI();
        if (currentLives <= 0) StartCoroutine(GameOverSequence());
    }

    // ============================================================
    // WIN PAUSE (scoreToWin reached)
    // ============================================================
    IEnumerator WinPauseSequence()
    {
        // only trigger once per game session
        if (isPaused) yield break;
        isPaused = true;

        if (winSound != null) audioSource.PlayOneShot(winSound, soundVolume);
        yield return new WaitForSeconds(0.5f);

        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (continuePanel != null) continuePanel.SetActive(true);

        if (continueYesButton != null)
        {
            continueYesButton.onClick.RemoveAllListeners();
            continueYesButton.onClick.AddListener(OnContinueYes);
        }
        if (continueNoButton != null)
        {
            continueNoButton.onClick.RemoveAllListeners();
            continueNoButton.onClick.AddListener(OnContinueNo);
        }
    }

    void OnContinueYes()
    {
        // keep playing until lives run out
        if (continuePanel != null) continuePanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
        isPaused = false;
        // reset score target so it doesn't retrigger immediately
        scoreToWin = int.MaxValue;
    }

    void OnContinueNo()
    {
        // exit to desktop
        isPlaying = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        ClearAllObjects();
        if (continuePanel != null) continuePanel.SetActive(false);
        AddToLeaderboard(currentScore);
        ReturnToDesktop();
    }

    // ============================================================
    // GAME OVER
    // ============================================================
    IEnumerator GameOverSequence()
    {
        isPlaying = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        ClearAllObjects();

        if (gameOverSound != null) audioSource.PlayOneShot(gameOverSound, soundVolume);
        AddToLeaderboard(currentScore);

        yield return new WaitForSeconds(0.8f);
        ShowLeaderboard();
    }

    // ============================================================
    // LEADERBOARD
    // ============================================================
    void AddToLeaderboard(int score)
    {
        leaderboardScores.Add(score);
        leaderboardScores.Sort((a, b) => b.CompareTo(a));
    }

    void ShowLeaderboard()
    {
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);

        if (leaderboardText != null)
        {
            string txt = "=== TOP SCORES ===\n\n";
            int count = Mathf.Min(leaderboardScores.Count, 10);
            for (int i = 0; i < count; i++)
                txt += (i + 1) + ". " + leaderboardScores[i].ToString("D4") + "\n";
            leaderboardText.text = txt;
        }

        if (leaderboardCloseButton != null)
        {
            leaderboardCloseButton.onClick.RemoveAllListeners();
            leaderboardCloseButton.onClick.AddListener(() =>
            {
                if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
                ReturnToDesktop();
            });
        }
    }

    // ============================================================
    // RETURN TO DESKTOP
    // ============================================================
    void ReturnToDesktop()
    {
        // notify PSDesktop to show itself again
        if (PSDesktop.Instance != null)
            PSDesktop.Instance.ReturnToDesktop();
    }

    // ============================================================
    // UI REFRESH
    // ============================================================
    void RefreshScoreUI()
    {
        if (scoreText != null) scoreText.text = currentScore.ToString("D4");
    }

    void RefreshLivesUI()
    {
        SetLifeIcon(lifeIcon1, currentLives >= 1);
        SetLifeIcon(lifeIcon2, currentLives >= 2);
        SetLifeIcon(lifeIcon3, currentLives >= 3);
    }

    void SetLifeIcon(Image icon, bool alive)
    {
        if (icon == null) return;
        if (alive && lifeActiveSprite != null) icon.sprite = lifeActiveSprite;
        if (!alive && lifeLostSprite != null) icon.sprite = lifeLostSprite;
        icon.color = alive ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
    }

    // ============================================================
    // CLEANUP
    // ============================================================
    void ClearAllObjects()
    {
        foreach (var fo in activeObjects)
            if (fo.rect != null) Destroy(fo.rect.gameObject);
        activeObjects.Clear();
    }

    void OnDisable()
    {
        isPlaying = false;
        ClearAllObjects();
    }
}