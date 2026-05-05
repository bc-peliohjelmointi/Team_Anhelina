using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// exit portal - player touches this point to leave the scene
// only becomes active after all required tasks are done
// visually transforms from one state to another (inactive ? active)
// when active: glows, shows "E - Exit" prompt
// player presses E ? fade out ? next scene loads
// place this anywhere in the world - doorway, hallway end, etc.
public class ExitPortal : MonoBehaviour
{
    // ---- scene to load ----
    // name of the next scene, must be added to Build Settings
    public string nextSceneName = "Scene2";

    // ---- activation condition ----
    // portal only activates after GameFlowManager reaches this stage
    // set in inspector - matches GameFlowManager.GameStage enum value
    // AlarmOff = 5, ExitReady = 6
    public GameFlowManager gameFlowManager;

    // ---- visual transformation ----
    // the inactive visual (e.g. plain door frame, dark object)
    public GameObject inactiveVisual;
    // the active visual (e.g. glowing portal, lit doorway)
    public GameObject activeVisual;
    // how fast the transform lerp happens
    public float activationSpeed = 2f;

    // ---- interaction ----
    public float interactionDistance = 2f;
    public KeyCode useKey = KeyCode.E;
    public GameObject interactionPrompt;
    public AuraHighlight auraHighlight;

    // ---- audio ----
    public AudioSource audioSource;
    // hum sound when portal is active
    public AudioClip activeHumSound;
    // sound when player uses portal
    public AudioClip useSound;
    public float soundVolume = 0.7f;

    // ---- state ----
    private bool isActive = false;
    private bool hasBeenUsed = false;
    private bool isNearby = false;

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        // start in inactive state
        if (inactiveVisual != null) inactiveVisual.SetActive(true);
        if (activeVisual != null) activeVisual.SetActive(false);
        if (auraHighlight != null) auraHighlight.SetGlow(false);
    }

    void Update()
    {
        if (hasBeenUsed) return;
        // check if conditions are met to activate portal
        CheckActivation();
        if (!isActive) return;
        CheckPlayerProximity();
    }

    void CheckActivation()
    {
        if (isActive) return;
        if (gameFlowManager == null) return;
        // activate when alarm is off (player has card and swiped alarm reader)
        bool shouldActivate =
            gameFlowManager.currentStage == GameFlowManager.GameStage.AlarmOff ||
            gameFlowManager.currentStage == GameFlowManager.GameStage.ExitReady;
        if (shouldActivate) Activate();
    }

    void Activate()
    {
        isActive = true;
        // swap visuals
        if (inactiveVisual != null) inactiveVisual.SetActive(false);
        if (activeVisual != null) activeVisual.SetActive(true);
        if (auraHighlight != null) auraHighlight.SetGlow(true);
        // play ambient hum
        if (activeHumSound != null)
        {
            audioSource.clip = activeHumSound;
            audioSource.loop = true;
            audioSource.volume = soundVolume * 0.5f;
            audioSource.Play();
        }
    }

    void CheckPlayerProximity()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        bool lookingAt = Physics.Raycast(
            new Ray(cam.transform.position, cam.transform.forward),
            out RaycastHit hit, interactionDistance)
            && hit.collider.gameObject == gameObject;
        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }
        if (isNearby && Input.GetKeyDown(useKey)) UsePortal();
    }

    void UsePortal()
    {
        if (hasBeenUsed) return;
        hasBeenUsed = true;
        audioSource.Stop();
        if (useSound != null) audioSource.PlayOneShot(useSound, soundVolume);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (auraHighlight != null) auraHighlight.SetGlow(false);
        // notify game flow
        if (gameFlowManager != null) gameFlowManager.OnExitUnlocked();
        // fade and load next scene
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.FadeTo(nextSceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }

    // draw a cyan sphere in scene view so you can see portal placement
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}