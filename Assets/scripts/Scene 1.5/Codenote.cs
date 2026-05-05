using UnityEngine;
using UnityEngine.UI;
// a note or poster somewhere in the scene with the 8-digit access code written on it
// player looks at it and presses E to "memorize" the code onto their clipboard
// code is taken from SceneCodeManager so it matches what PC expects
// the note can be a world-space canvas on a wall, desk, or sticky note object
// after reading, the clipboard shows the code and task updates
public class CodeNote : MonoBehaviour
{
    // how far player can be to read the note
    public float readDistance = 2.5f;
    public KeyCode readKey = KeyCode.E;
    // "E - ?????????" prompt shown when player looks at note
    public GameObject interactionPrompt;
    // the Text component on the note that shows the actual code
    // this gets filled automatically from SceneCodeManager
    public Text noteCodeText;
    // "???: 12345678" label format
    public string codePrefix = "??? ???????: ";
    // optional second line of flavour text on the note
    public string flavourText = "\n??? ?????????? ???????????";
    // aura glow on this object
    public AuraHighlight auraHighlight;
    // optional - panel on clipboard that shows the code after reading
    // if assigned, this panel activates when player reads the note
    public GameObject clipboardCodePanel;
    // text inside clipboard that shows the copied code
    public Text clipboardCodeText;

    public AudioSource audioSource;
    // paper rustling sound
    public AudioClip readSound;
    public float soundVolume = 0.6f;

    private bool hasBeenRead = false;
    private bool isNearby = false;
    private string theCode = "";

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (clipboardCodePanel != null) clipboardCodePanel.SetActive(false);
        // get the code from SceneCodeManager and display it on the note
        if (SceneCodeManager.Instance != null)
            theCode = SceneCodeManager.Instance.GetCode();
        else
            theCode = "????????";
        if (noteCodeText != null)
            noteCodeText.text = codePrefix + theCode + flavourText;
    }

    void Update()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        bool lookingAt = Physics.Raycast(
            new Ray(cam.transform.position, cam.transform.forward),
            out RaycastHit hit, readDistance)
            && hit.collider.gameObject == gameObject;
        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (auraHighlight != null) auraHighlight.SetGlow(isNearby);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }
        if (isNearby && Input.GetKeyDown(readKey)) ReadNote();
    }

    void ReadNote()
    {
        if (readSound != null) audioSource.PlayOneShot(readSound, soundVolume);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (auraHighlight != null) auraHighlight.SetGlow(false);
        // show code on clipboard
        if (clipboardCodePanel != null) clipboardCodePanel.SetActive(true);
        if (clipboardCodeText != null)
            clipboardCodeText.text = "??? ???????:\n" + theCode;
        if (!hasBeenRead)
        {
            hasBeenRead = true;
            // update task
            if (TaskManager.Instance != null)
                TaskManager.Instance.CompleteTask(TaskManager.Instance.task_FindCode);
        }
    }

    // returns true if player has already read this note
    public bool HasBeenRead() => hasBeenRead;
}