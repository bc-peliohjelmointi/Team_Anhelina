using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ComputerInteraction : MonoBehaviour
{
    public DrawerSystem drawerSystem;
    public PlayerMovement playerMovement;
    public float interactionDistance = 2.5f;
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactionPrompt;
    public GameObject computerUI;
    public Text displayText;
    public Text feedbackText;
    private bool isNearby = false, isUsingComputer = false, codeAlreadyEntered = false;
    private string enteredCode = "", correctCode = "00000000";
    private AuraHighlight aura;

    void Start()
    {
        aura = GetComponent<AuraHighlight>();
        if (computerUI != null) computerUI.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        if (feedbackText != null) feedbackText.text = "";
        if (CodeManager.Instance != null) correctCode = CodeManager.Instance.GetCode();
    }

    void Update()
    {
        if (isUsingComputer) { if (Input.GetKeyDown(KeyCode.Escape)) ExitComputer(); return; }
        Camera cam = Camera.main;
        if (cam == null) return;
        bool lookingAt = Physics.Raycast(new Ray(cam.transform.position, cam.transform.forward), out RaycastHit hit, interactionDistance)
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
        isUsingComputer = true; enteredCode = "";
        UpdateDisplay();
        if (feedbackText != null) feedbackText.text = "";
        if (computerUI != null) computerUI.SetActive(true);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        if (playerMovement != null) playerMovement.LockControl();
    }

    public void ExitComputer()
    {
        isUsingComputer = false;
        if (computerUI != null) computerUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        if (playerMovement != null) playerMovement.UnlockControl();
    }

    public void PressDigit(string digit) { if (enteredCode.Length < 8) { enteredCode += digit; UpdateDisplay(); } }
    public void PressBackspace() { if (enteredCode.Length > 0) { enteredCode = enteredCode.Substring(0, enteredCode.Length - 1); UpdateDisplay(); } }

    public void PressConfirm()
    {
        if (codeAlreadyEntered) { ShowFeedback("???? ??? ??????", Color.yellow); return; }
        if (enteredCode.Length < 8) { ShowFeedback("??????? 8 ????", Color.yellow); return; }
        if (enteredCode == correctCode)
        {
            codeAlreadyEntered = true;
            ShowFeedback("?????? ????????", Color.green);
            if (drawerSystem != null) drawerSystem.OpenCardDrawer();
            if (TaskManager.Instance != null) TaskManager.Instance.CompleteTask(TaskManager.Instance.task_FindCode);
            StartCoroutine(CloseAfterDelay(1.5f));
        }
        else { ShowFeedback("???????? ???", Color.red); enteredCode = ""; UpdateDisplay(); }
    }

    void UpdateDisplay()
    {
        if (displayText == null) return;
        string d = enteredCode; while (d.Length < 8) d += "_"; displayText.text = d;
    }

    void ShowFeedback(string msg, Color col) { if (feedbackText != null) { feedbackText.text = msg; feedbackText.color = col; } }

    IEnumerator CloseAfterDelay(float delay) { yield return new WaitForSeconds(delay); ExitComputer(); }
}