using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
// ============================================================
// NU POGODI MINI-GAME — full Game Boy style arcade
// ============================================================
// CONTROLS: A = top-left, Z = bottom-left, D = top-right, X = bottom-right
// Wolf stands in center, catches eggs falling in 4 columns
// Each column has TWO zones: top and bottom
// LIVES: 3 rabbit-head icons — catching a rabbit head = screamer + lose a life
//        dropping an egg = lose 1 life (no screamer)
// SCORING: each caught egg = 1 point, score target set in inspector
// FIRST WIN: drawer opens, game pauses → "Continue? Yes/No"
//   Yes → game keeps going until all lives lost → leaderboard shown
//   No  → exit to desktop, game icon appears on desktop
// SUBSEQUENT WINS: no teleport, no fade, just leaderboard entry added
// GAME OVER: first time → screen fades, player teleports to office entry
//            after first win achieved → just show leaderboard, no teleport
// called by ComputerInteraction after code is accepted
public class NuPogodeMiniGame : MonoBehaviour
{
    // ---- score settings ----
    // how many points needed to unlock the drawer - change in inspector
    public int scoreToWin = 100;

    // ---- UI panels ----
    // root panel of the entire mini-game
    public RectTransform gamePanel;
    // shown while game is running
    public GameObject gameplayPanel;
    // shown when asking "Continue? Yes/No" after first win
    public GameObject continuePanel;
    // shown at game over with leaderboard
    public GameObject leaderboardPanel;
    // desktop panel shown after exiting
    public GameObject desktopPanel;
    // game icon button on desktop, hidden until player has played once
    public GameObject gameIconButton;
    // flashing "PROYDITE TEST NA INOAGENTA" text shown after code accepted
    public Text flashingInstructionText;

    // ---- gameplay UI ----
    // score display "0000" in top center, Game Boy style
    public Text scoreText;
    // 3 rabbit head images used as life icons
    public Image lifeIcon1, lifeIcon2, lifeIcon3;
    // sprite shown when life is active
    public Sprite lifeActiveSprite;
    // sprite shown when life is lost (darkened or crossed out)
    public Sprite lifeLostSprite;
    // full screen screamer image - scary robot rabbit face
    public Image screamerImage;
    // full screen dark overlay for screamer flash
    public Image darkFlashOverlay;
    // full screen fade overlay for game over teleport
    public Image fadeOverlay;

    // ---- leaderboard UI ----
    // text showing top scores inside leaderboard panel
    public Text leaderboardText;
    // X button to close leaderboard and return to desktop
    public Button leaderboardCloseButton;

    // ---- continue panel UI ----
    public Button continueYesButton;
    public Button continueNoButton;

    // ---- column spawning ----
    // 4 spawn points at the TOP of each column (left to right)
    public RectTransform spawnTop1, spawnTop2, spawnTop3, spawnTop4;
    // 4 spawn points at the BOTTOM of each column
    public RectTransform spawnBot1, spawnBot2, spawnBot3, spawnBot4;
    // prefab: egg sprite Image
    public GameObject eggPrefab;
    // prefab: robot rabbit head sprite Image (scary)
    public GameObject rabbitHeadPrefab;
    // parent where falling objects are instantiated
    public RectTransform fallingObjectsParent;

    // ---- wolf catch zones ----
    // 4 catch zone positions top row (A=left, D=right)
    public RectTransform catchZoneTopLeft;
    public RectTransform catchZoneTopRight;
    // 4 catch zone positions bottom row (Z=left, X=right)
    public RectTransform catchZoneBotLeft;
    public RectTransform catchZoneBotRight;
    // wolf images for each position (or one image that moves)
    public Image wolfTopLeft, wolfTopRight, wolfBotLeft, wolfBotRight;
    // how wide each catch zone is in pixels
    public float catchRadius = 60f;

    // ---- difficulty ----
    // starting fall speed in pixels per second
    public float startFallSpeed = 180f;
    // how much speed increases per 10 points scored
    public float speedIncreasePerTen = 20f;
    // max fall speed cap
    public float maxFallSpeed = 600f;
    // time between egg spawns at start
    public float startSpawnInterval = 1.4f;
    // minimum spawn interval (fastest difficulty)
    public float minSpawnInterval = 0.4f;
    // chance 0-1 that a spawned object is a rabbit head
    public float baseRabbitChance = 0.15f;

    // ---- screamer ----
    public float screamerDuration = 0.7f;
    public AudioSource audioSource;
    public AudioClip screamerSound;
    public AudioClip catchEggSound;
    public AudioClip dropEggSound;
    public AudioClip gameOverSound;
    public AudioClip winSound;
    public float soundVolume = 0.8f;

    // ---- checkpoint reference ----
    // used for first-time game over teleport
    public CheckpointManager checkpointManager;

    // ---- state ----
    private int currentScore = 0;
    private int currentLives = 3;
    private bool isPlaying = false;
    private bool isPaused = false;
    private bool firstWinAchieved = false;
    private bool drawerUnlocked = false;
    private float currentFallSpeed;
    private float currentSpawnInterval;
    private Coroutine spawnCoroutine;
    private Coroutine flashCoroutine;
    private ComputerInteraction computerRef;
    // leaderboard stored as simple list of scores
    private List<int> leaderboardScores = new List<int>();
    // which wolf position is currently active
    // positions: 0=TopLeft, 1=TopRight, 2=BotLeft, 3=BotRight
    private int activeWolfPos = 2; // starts bottom-left

    private class FallingObj
    {
        public RectTransform rect;
        public int column;      // 0-3
        public bool fromTop;    // true = falling from top down, false = from bottom up
        public bool isRabbit;
        public bool done;
    }
    private List<FallingObj> activeObjects = new List<FallingObj>();

    // ---- public entry point ----
    // called by ComputerInteraction after code is accepted
    public void StartGame(ComputerInteraction computer)
    {
        computerRef = computer;
        // show flashing instruction text first
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(ShowFlashingInstruction());
    }

    IEnumerator ShowFlashingInstruction()
    {
        if (flashingInstructionText != null)
        {
            flashingInstructionText.gameObject.SetActive(true);
            flashingInstructionText.text = "PROYDITE TEST NA INOAGENTA\nNABERITE " + scoreToWin + " OCHKOV";
            // flash 6 times then start game
            for (int i = 0; i < 6; i++)
            {
                flashingInstructionText.enabled = !flashingInstructionText.enabled;
                yield return new WaitForSeconds(0.4f);
            }
            flashingInstructionText.enabled = false;
            flashingInstructionText.gameObject.SetActive(false);
        }
        yield return new WaitForSeconds(0.3f);
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
        activeWolfPos = 2; // bottom-left default
        ClearAllObjects();
        RefreshScoreUI();
        RefreshLivesUI();
        SetWolfPosition(activeWolfPos);
        if (screamerImage != null) screamerImage.gameObject.SetActive(false);
        if (darkFlashOverlay != null) darkFlashOverlay.gameObject.SetActive(false);
        if (fadeOverlay != null) fadeOverlay.color = new Color(0, 0, 0, 0);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
        if (continuePanel != null) continuePanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

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
        // A = top-left, Z = bottom-left, D = top-right, X = bottom-right
        if (Input.GetKeyDown(KeyCode.A)) SetWolfPosition(0);
        if (Input.GetKeyDown(KeyCode.Z)) SetWolfPosition(2);
        if (Input.GetKeyDown(KeyCode.D)) SetWolfPosition(1);
        if (Input.GetKeyDown(KeyCode.X)) SetWolfPosition(3);
    }

    void SetWolfPosition(int pos)
    {
        activeWolfPos = pos;
        // show only the active wolf image
        if (wolfTopLeft != null) wolfTopLeft.gameObject.SetActive(pos == 0);
        if (wolfTopRight != null) wolfTopRight.gameObject.SetActive(pos == 1);
        if (wolfBotLeft != null) wolfBotLeft.gameObject.SetActive(pos == 2);
        if (wolfBotRight != null) wolfBotRight.gameObject.SetActive(pos == 3);
    }

    void UpdateDifficulty()
    {
        // increase speed every 10 points
        float speedBoost = (currentScore / 10) * speedIncreasePerTen;
        currentFallSpeed = Mathf.Min(startFallSpeed + speedBoost, maxFallSpeed);
        // decrease spawn interval as score rises
        float intervalReduction = (currentScore / 10) * 0.08f;
        currentSpawnInterval = Mathf.Max(startSpawnInterval - intervalReduction, minSpawnInterval);
    }

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
        // pick random column 0-3
        int col = Random.Range(0, 4);
        // pick top or bottom spawn randomly
        bool fromTop = Random.value > 0.5f;
        bool isRabbit = Random.value < (baseRabbitChance + currentScore * 0.001f);
        GameObject prefab = isRabbit ? rabbitHeadPrefab : eggPrefab;
        if (prefab == null || fallingObjectsParent == null) return;
        GameObject go = Instantiate(prefab, fallingObjectsParent);
        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt == null) { Destroy(go); return; }
        // position at the correct spawn point
        RectTransform spawnPoint = GetSpawnPoint(col, fromTop);
        if (spawnPoint != null) rt.anchoredPosition = spawnPoint.anchoredPosition;
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

    void TickObjects()
    {
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            FallingObj fo = activeObjects[i];
            if (fo.done || fo.rect == null) { activeObjects.RemoveAt(i); continue; }
            Vector2 p = fo.rect.anchoredPosition;
            // top objects fall downward, bottom objects rise upward
            p.y += (fo.fromTop ? -1f : 1f) * currentFallSpeed * Time.deltaTime;
            fo.rect.anchoredPosition = p;
            // check if object passed through without being caught
            bool missed = fo.fromTop ? (p.y < -500f) : (p.y > 500f);
            if (missed)
            {
                fo.done = true;
                Destroy(fo.rect.gameObject);
                activeObjects.RemoveAt(i);
                if (!fo.isRabbit)
                {
                    // dropped egg = lose 1 life, no screamer
                    if (dropEggSound != null) audioSource.PlayOneShot(dropEggSound, soundVolume);
                    LoseLife(false);
                }
                // missed rabbit head just disappears, no penalty
            }
        }
    }

    void CheckCatches()
    {
        // determine which catch zones are active based on wolf position
        // wolf pos: 0=TopLeft, 1=TopRight, 2=BotLeft, 3=BotRight
        RectTransform activeZone = null;
        int activeCols = -1;
        bool activeTop = false;
        switch (activeWolfPos)
        {
            case 0: activeZone = catchZoneTopLeft; activeCols = 0; activeTop = true; break;
            case 1: activeZone = catchZoneTopRight; activeCols = 3; activeTop = true; break;
            case 2: activeZone = catchZoneBotLeft; activeCols = 0; activeTop = false; break;
            case 3: activeZone = catchZoneBotRight; activeCols = 3; activeTop = false; break;
        }
        if (activeZone == null) return;
        // wolf at top-left covers columns 0,1 top; top-right covers 2,3 top
        // wolf at bot-left covers columns 0,1 bottom; bot-right covers 2,3 bottom
        int colMin = (activeWolfPos == 0 || activeWolfPos == 2) ? 0 : 2;
        int colMax = colMin + 1;
        Vector2 zonePos = activeZone.anchoredPosition;
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            FallingObj fo = activeObjects[i];
            if (fo.done || fo.rect == null) continue;
            // must match column range and direction
            if (fo.column < colMin || fo.column > colMax) continue;
            if (fo.fromTop != activeTop) continue;
            Vector2 op = fo.rect.anchoredPosition;
            if (Vector2.Distance(op, zonePos) < catchRadius)
            {
                fo.done = true;
                Destroy(fo.rect.gameObject);
                activeObjects.RemoveAt(i);
                if (fo.isRabbit) StartCoroutine(CaughtRabbit());
                else CaughtEgg();
            }
        }
    }

    void CaughtEgg()
    {
        if (catchEggSound != null) audioSource.PlayOneShot(catchEggSound, soundVolume);
        currentScore++;
        RefreshScoreUI();
        // check for win condition
        if (!drawerUnlocked && currentScore >= scoreToWin)
        {
            drawerUnlocked = true;
            StartCoroutine(FirstWinSequence());
        }
    }

    IEnumerator CaughtRabbit()
    {
        // show screamer
        if (screamerSound != null) audioSource.PlayOneShot(screamerSound, 1f);
        if (screamerImage != null) screamerImage.gameObject.SetActive(true);
        if (darkFlashOverlay != null)
        {
            darkFlashOverlay.gameObject.SetActive(true);
            darkFlashOverlay.color = new Color(0, 0, 0, 0.5f);
        }
        yield return new WaitForSeconds(screamerDuration);
        if (screamerImage != null) screamerImage.gameObject.SetActive(false);
        if (darkFlashOverlay != null) darkFlashOverlay.gameObject.SetActive(false);
        LoseLife(true); // true = came from rabbit catch
    }

    void LoseLife(bool fromRabbit)
    {
        currentLives--;
        RefreshLivesUI();
        if (currentLives <= 0) StartCoroutine(GameOverSequence());
    }

    IEnumerator FirstWinSequence()
    {
        isPaused = true;
        if (winSound != null) audioSource.PlayOneShot(winSound, soundVolume);
        // open the card drawer
        if (DrawerSystem.Instance != null) DrawerSystem.Instance.OpenCardDrawer();
        firstWinAchieved = true;
        yield return new WaitForSeconds(1f);
        // show continue panel
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (continuePanel != null) continuePanel.SetActive(true);
        // wire buttons
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
    }

    void OnContinueNo()
    {
        // exit to desktop, add score to leaderboard
        isPlaying = false;
        if (continuePanel != null) continuePanel.SetActive(false);
        leaderboardScores.Add(currentScore);
        leaderboardScores.Sort((a, b) => b.CompareTo(a));
        // show desktop with game icon
        ShowDesktop();
    }

    IEnumerator GameOverSequence()
    {
        isPlaying = false;
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        ClearAllObjects();
        if (gameOverSound != null) audioSource.PlayOneShot(gameOverSound, soundVolume);
        // add score to leaderboard
        leaderboardScores.Add(currentScore);
        leaderboardScores.Sort((a, b) => b.CompareTo(a));
        if (!firstWinAchieved)
        {
            // first time losing: fade screen and teleport player back to office entry
            yield return StartCoroutine(FadeOut());
            if (checkpointManager != null) checkpointManager.TriggerGameOver();
            else if (computerRef != null) computerRef.OnMiniGameLost();
        }
        else
        {
            // already won before: just show leaderboard, no teleport
            yield return new WaitForSeconds(0.5f);
            ShowLeaderboard();
        }
    }

    void ShowLeaderboard()
    {
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        // build leaderboard text
        if (leaderboardText != null)
        {
            string txt = "=== TOP SCORES ===\n\n";
            for (int i = 0; i < Mathf.Min(leaderboardScores.Count, 10); i++)
                txt += (i + 1) + ". " + leaderboardScores[i].ToString("D4") + "\n";
            leaderboardText.text = txt;
        }
        if (leaderboardCloseButton != null)
        {
            leaderboardCloseButton.onClick.RemoveAllListeners();
            leaderboardCloseButton.onClick.AddListener(OnLeaderboardClose);
        }
    }

    void OnLeaderboardClose()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
        ShowDesktop();
    }

    void ShowDesktop()
    {
        if (desktopPanel != null) desktopPanel.SetActive(true);
        if (gameplayPanel != null) gameplayPanel.SetActive(false);
        // show game icon so player can replay
        if (gameIconButton != null)
        {
            gameIconButton.SetActive(true);
            // wire icon click to restart game
            Button btn = gameIconButton.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(RestartFromDesktop);
            }
        }
    }

    void RestartFromDesktop()
    {
        if (desktopPanel != null) desktopPanel.SetActive(false);
        if (gameplayPanel != null) gameplayPanel.SetActive(true);
        InitGame();
    }

    IEnumerator FadeOut()
    {
        if (fadeOverlay == null) yield break;
        fadeOverlay.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1.5f;
            fadeOverlay.color = new Color(0, 0, 0, Mathf.Clamp01(t));
            yield return null;
        }
    }

    void RefreshScoreUI()
    {
        if (scoreText != null) scoreText.text = currentScore.ToString("D4");
    }

    void RefreshLivesUI()
    {
        // each life icon shows active or lost sprite
        RefreshOneLife(lifeIcon1, currentLives >= 1);
        RefreshOneLife(lifeIcon2, currentLives >= 2);
        RefreshOneLife(lifeIcon3, currentLives >= 3);
    }

    void RefreshOneLife(Image icon, bool alive)
    {
        if (icon == null) return;
        if (alive && lifeActiveSprite != null) icon.sprite = lifeActiveSprite;
        if (!alive && lifeLostSprite != null) icon.sprite = lifeLostSprite;
        icon.color = alive ? Color.white : new Color(0.3f, 0.3f, 0.3f, 1f);
    }

    void ClearAllObjects()
    {
        foreach (var fo in activeObjects)
            if (fo.rect != null) Destroy(fo.rect.gameObject);
        activeObjects.Clear();
    }

    void OnDisable() { isPlaying = false; ClearAllObjects(); }
}