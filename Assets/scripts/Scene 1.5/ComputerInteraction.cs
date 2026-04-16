using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening.Core.Easing;
// the computer terminal where player types in the code
// gets correct code from SceneCodeManager on scene load
// shows aura glow when player looks at it
// opens keyboard UI when player presses E
// on correct code opens the card drawer and updates task list
public class ComputerInteraction : MonoBehaviour
{
    // the drawer system to open when code is correct
    public DrawerSystem drawerSystem;
    // player movement script to lock controls while using computer
    public PlayerMovement playerMovement;
    // how close player needs to be to interact
    public float interactionDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;
    // small canvas above computer saying "E - Computer"
    public GameObject interactionPrompt;
    // the keyboard UI canvas, starts inactive
    public GameObject computerUI;
    // shows entered digits like "1234____"
    public Text displayText;
    // shows "Wrong code" or "Access granted" etc
    public Text feedbackText;
    private bool isNearby = false;
    private bool isUsingComputer = false;
    private bool codeAlreadyEntered = false;
    private string enteredCode = "";
    private string correctCode = "00000000";
    private AuraHighlight aura;

    void Start()
    {
        aura = GetComponent<AuraHighlight>();
        if (computerUI != null) computerUI.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";
        // grab the scene code, falls back to 00000000 if manager missing
        if (SceneCodeManager.Instance != null)
            correctCode = SceneCodeManager.Instance.GetCode();
    }

    void Update()
    {
        if (isUsingComputer)
        {
            // escape closes the keyboard UI
            if (Input.GetKeyDown(KeyCode.Escape)) ExitComputer();
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        bool lookingAt = Physics.Raycast(
            new Ray(cam.transform.position, cam.transform.forward),
            out RaycastHit hit, interactionDistance)
            && hit.collider.gameObject == gameObject;

        if (lookingAt != isNearby)
        {
            isNearby = lookingAt;
            if (aura != null) aura.SetGlow(isNearby);
            if (interactionPrompt != null) interactionPrompt.SetActive(isNearby);
        }

        if (isNearby && Input.GetKeyDown(interactKey)) EnterComputer();
    }

    void EnterComputer()
    {
        isUsingComputer = true;
        enteredCode = "";
        UpdateDisplay();
        if (feedbackText != null) feedbackText.text = "";
        if (computerUI != null) computerUI.SetActive(true);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        // unlock cursor so player can click keyboard buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerMovement != null) playerMovement.LockControl();
    }

    public void ExitComputer()
    {
        isUsingComputer = false;
        if (computerUI != null) computerUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerMovement != null) playerMovement.UnlockControl();
    }

    // hooked up to each number button in the keyboard UI
    public void PressDigit(string digit)
    {
        if (enteredCode.Length < 8) { enteredCode += digit; UpdateDisplay(); }
    }

    public void PressBackspace()
    {
        if (enteredCode.Length > 0)
        {
            enteredCode = enteredCode.Substring(0, enteredCode.Length - 1);
            UpdateDisplay();
        }
    }

    public void PressConfirm()
    {
        if (codeAlreadyEntered) { ShowFeedback("Drawer already open", Color.yellow); return; }
        if (enteredCode.Length < 8) { ShowFeedback("Enter 8 digits", Color.yellow); return; }

        if (enteredCode == correctCode)
        {
            codeAlreadyEntered = true;
            ShowFeedback("Access granted", Color.green);
            if (drawerSystem != null) drawerSystem.OpenCardDrawer();
            if (TaskManager.Instance != null)
                TaskManager.Instance.CompleteTask(TaskManager.Instance.task_FindCode);
            StartCoroutine(CloseAfterDelay(1.5f));
        }
        else
        {
            ShowFeedback("Wrong code", Color.red);
            enteredCode = "";
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        if (displayText == null) return;
        // pad with underscores to always show 8 character slots
        string d = enteredCode;
        while (d.Length < 8) d += "_";
        displayText.text = d;
    }

    void ShowFeedback(string msg, Color col)
    {
        if (feedbackText == null) return;
        feedbackText.text = msg;
        feedbackText.color = col;
    }

    IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ExitComputer();
    }
}