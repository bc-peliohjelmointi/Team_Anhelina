using UnityEngine;
// stripped down version of ObjectDragRay specifically for the lever scene

// keep this on the camera or player head object
public class ObjectDragRayLevers : MonoBehaviour
{
    // how far the raycast reaches, 6 units is fine for most rooms
    public float maxDistance = 6f;
    // crosshair dot size in screen pixels
    public int dotSize = 4;
    public Color dotColor = Color.white;
    public bool showCrosshair = true;
    // main interaction key
    public KeyCode interactKey = KeyCode.E;
    // shorter range for puzzle panel detection
    public float puzzlePanelDistance = 3f;
    // the "E - use panel" UI prompt object
    public GameObject puzzlePanelPrompt;
    private Texture2D dotTexture;
    // puzzle panel we're currently looking at
    private PuzzlePanelInteraction currentPanel;
    // which object is currently glowing
    private AuraHighlight currentAura;

    void Awake()
    {
        // create the small crosshair dot texture
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, dotColor);
        dotTexture.Apply();
        if (puzzlePanelPrompt != null) puzzlePanelPrompt.SetActive(false);
    }

    void Update()
    {
        // check these every frame regardless
        CheckForPuzzlePanel();
        CheckForAura();

        // enter panel mode if player presses E while looking at a panel
        if (Input.GetKeyDown(interactKey))
        {
            if (currentPanel != null)
            {
                currentPanel.EnterPuzzleMode();
                return;
            }
        }
    }

    // detects PuzzlePanelInteraction within short range
    void CheckForPuzzlePanel()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, puzzlePanelDistance))
        {
            PuzzlePanelInteraction panel = hit.collider.GetComponent<PuzzlePanelInteraction>();
            if (panel != null)
            {
                if (currentPanel != panel)
                {
                    currentPanel = panel;
                    if (puzzlePanelPrompt != null) puzzlePanelPrompt.SetActive(true);
                }
                return;
            }
        }
        ClearPanel();
    }

    // checks full ray distance for AuraHighlight on whatever player is looking at
    void CheckForAura()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        AuraHighlight aura = null;
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            aura = hit.collider.GetComponent<AuraHighlight>();

        // only update if its different from last frame
        if (aura != currentAura)
        {
            if (currentAura != null) currentAura.SetGlow(false);
            currentAura = aura;
            if (currentAura != null) currentAura.SetGlow(true);
        }
    }

    void ClearPanel()
    {
        if (currentPanel == null) return;
        currentPanel = null;
        if (puzzlePanelPrompt != null) puzzlePanelPrompt.SetActive(false);
    }

    void OnGUI()
    {
        if (!showCrosshair) return;
        GUI.DrawTexture(new Rect(
            (Screen.width - dotSize) * 0.5f,
            (Screen.height - dotSize) * 0.5f,
            dotSize, dotSize), dotTexture);
    }

    void OnDisable()
    {
        // important - always clean up state when disabled
        ClearPanel();
        if (currentAura != null) { currentAura.SetGlow(false); currentAura = null; }
    }
}