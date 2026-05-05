using UnityEngine;
using UnityEngine.UI;
using System.Collections;
// DOS-style folder system shown on the PC screen before code entry
// five folders displayed as text buttons on black background
// four folders show "NO ACCESS" error when clicked
// one correct folder opens the code input panel
// folders are shown first, code panel is hidden until correct folder clicked
// style: green text on black background, retro CRT look
public class PCFolderSystem : MonoBehaviour
{
    // ---- panels ----
    // the folder selection panel shown first
    public GameObject folderPanel;
    // the code input panel shown after correct folder is clicked
    public GameObject codePanel;

    // ---- folder buttons (assign 5 Button components) ----
    public Button folder1Button;
    public Button folder2Button;
    public Button folder3Button;
    public Button folder4Button;
    public Button folder5Button;

    // ---- folder name labels ----
    public Text folder1Label;
    public Text folder2Label;
    public Text folder3Label;
    public Text folder4Label;
    public Text folder5Label;

    // folder names shown on screen - change in inspector to fit your story
    public string folder1Name = "> OTCHETY_2031/";
    public string folder2Name = "> PERSONAL/";
    public string folder3Name = "> SISTEMA/";
    public string folder4Name = "> ARHIV_STARYY/";
    // this is the correct folder that opens the code panel
    public string folder5Name = "> LICHNOE_DOSTUP/";

    // which folder number is the correct one (1-5)
    // change this in inspector, default is folder 5
    public int correctFolderIndex = 5;

    // ---- error feedback ----
    // text that shows "DOSTUP ZAPRESHCHEN" when wrong folder clicked
    public Text errorText;
    // how long error text stays visible
    public float errorDuration = 2f;

    // ---- audio ----
    public AudioSource audioSource;
    // old keyboard click sound
    public AudioClip keyClickSound;
    // error beep for wrong folder
    public AudioClip errorSound;
    // successful folder open sound
    public AudioClip openSound;
    public float soundVolume = 0.6f;

    // ---- cursor blink ----
    // optional blinking cursor text at bottom of screen
    public Text cursorText;
    private float cursorTimer;
    private bool cursorVisible = true;

    private Coroutine errorCoroutine;

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        // set folder names
        SetFolderLabels();
        // hide error text
        if (errorText != null) { errorText.text = ""; errorText.gameObject.SetActive(false); }
        // show folder panel, hide code panel
        if (folderPanel != null) folderPanel.SetActive(true);
        if (codePanel != null) codePanel.SetActive(false);
        // wire up button clicks
        if (folder1Button != null) folder1Button.onClick.AddListener(() => OnFolderClicked(1));
        if (folder2Button != null) folder2Button.onClick.AddListener(() => OnFolderClicked(2));
        if (folder3Button != null) folder3Button.onClick.AddListener(() => OnFolderClicked(3));
        if (folder4Button != null) folder4Button.onClick.AddListener(() => OnFolderClicked(4));
        if (folder5Button != null) folder5Button.onClick.AddListener(() => OnFolderClicked(5));
    }

    void Update()
    {
        // blink the cursor every 0.5 seconds like a real terminal
        if (cursorText == null) return;
        cursorTimer += Time.deltaTime;
        if (cursorTimer >= 0.5f)
        {
            cursorTimer = 0f;
            cursorVisible = !cursorVisible;
            cursorText.text = cursorVisible ? "_" : " ";
        }
    }

    void SetFolderLabels()
    {
        if (folder1Label != null) folder1Label.text = folder1Name;
        if (folder2Label != null) folder2Label.text = folder2Name;
        if (folder3Label != null) folder3Label.text = folder3Name;
        if (folder4Label != null) folder4Label.text = folder4Name;
        if (folder5Label != null) folder5Label.text = folder5Name;
    }

    void OnFolderClicked(int index)
    {
        if (keyClickSound != null) audioSource.PlayOneShot(keyClickSound, soundVolume);
        if (index == correctFolderIndex)
        {
            // correct folder - open code panel
            if (openSound != null) audioSource.PlayOneShot(openSound, soundVolume);
            if (folderPanel != null) folderPanel.SetActive(false);
            if (codePanel != null) codePanel.SetActive(true);
        }
        else
        {
            // wrong folder - show access denied error
            if (errorCoroutine != null) StopCoroutine(errorCoroutine);
            errorCoroutine = StartCoroutine(ShowError(index));
        }
    }

    IEnumerator ShowError(int folderIndex)
    {
        if (errorSound != null) audioSource.PlayOneShot(errorSound, soundVolume);
        if (errorText != null)
        {
            errorText.gameObject.SetActive(true);
            // show which folder was denied in terminal style
            string folderName = GetFolderName(folderIndex).Replace("> ", "").Replace("/", "");
            errorText.text = "OSHIBKA: NET DOSTUPA K " + folderName + "\nTREBUETSYA AVTORIZACIYA UROVNYA 5";
        }
        yield return new WaitForSeconds(errorDuration);
        if (errorText != null) { errorText.text = ""; errorText.gameObject.SetActive(false); }
        errorCoroutine = null;
    }

    string GetFolderName(int index)
    {
        switch (index)
        {
            case 1: return folder1Name;
            case 2: return folder2Name;
            case 3: return folder3Name;
            case 4: return folder4Name;
            case 5: return folder5Name;
            default: return "UNKNOWN";
        }
    }

    // called by ComputerInteraction when PC mode is entered
    // resets back to folder view if player exits and re-enters
    public void ResetToFolders()
    {
        if (folderPanel != null) folderPanel.SetActive(true);
        if (codePanel != null) codePanel.SetActive(false);
        if (errorText != null) { errorText.text = ""; errorText.gameObject.SetActive(false); }
    }
}