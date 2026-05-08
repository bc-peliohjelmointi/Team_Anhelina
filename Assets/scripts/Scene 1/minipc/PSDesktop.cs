using UnityEngine;
using UnityEngine.UI;
// ============================================================
// PSDesktop.cs
// ============================================================
// Put this on the desktop Canvas (World Space, on the monitor quad)
// Two icon buttons detected via raycast from ObjectDragRay
// Icon 1: opens video panel
// Icon 2: opens game loading screen -> launches Nu Pogodi
//
// Icons are child GameObjects with PSDesktopIcon component
// Clicking them calls OnIconClicked(iconIndex)
//
// SETUP:
// - PSDesktop on the Canvas root
// - PSDesktopIcon on each icon GameObject (with Collider)
// - PSVideoPanel separate child panel
// - PSMiniGamePanel separate child panel
// ============================================================
public class PSDesktop : MonoBehaviour
{
    public static PSDesktop Instance { get; private set; }

    [Header("Panels")]
    // video panel shown when video icon is clicked
    public PSVideoPanel videoPanel;
    // mini-game panel with the Nu Pogodi game
    public GameObject miniGamePanel;
    public NuPogodeMiniGame miniGame;
    // loading screen shown before game starts
    public PSLoadingScreen loadingScreen;

    [Header("Desktop Background")]
    // Quad with your desktop wallpaper texture - assign in inspector
    public Renderer desktopQuad;

    [Header("Icons")]
    // the two icon objects - assign in inspector
    public PSDesktopIcon videoIcon;
    public PSDesktopIcon gameIcon;

    [Header("Selected Icon Highlight")]
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    private PSDesktopIcon currentSelected;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
        if (videoPanel != null) videoPanel.gameObject.SetActive(false);
        if (loadingScreen != null) loadingScreen.gameObject.SetActive(false);
    }

    // called by PSDesktopIcon when player clicks on it
    public void OnIconClicked(PSDesktopIcon icon)
    {
        if (icon == videoIcon) OpenVideo();
        else if (icon == gameIcon) StartCoroutine(OpenGame());
    }

    void OpenVideo()
    {
        if (videoPanel != null) videoPanel.Open();
    }

    System.Collections.IEnumerator OpenGame()
    {
        // show loading screen first
        if (loadingScreen != null)
        {
            loadingScreen.gameObject.SetActive(true);
            yield return loadingScreen.PlayLoadingSequence();
            loadingScreen.gameObject.SetActive(false);
        }
        // start the game
        if (miniGamePanel != null) miniGamePanel.SetActive(true);
        if (miniGame != null) miniGame.StartGame();
    }

    // called by video panel or game when they want to return to desktop
    public void ReturnToDesktop()
    {
        if (miniGamePanel != null) miniGamePanel.SetActive(false);
        if (videoPanel != null) videoPanel.gameObject.SetActive(false);
    }
}