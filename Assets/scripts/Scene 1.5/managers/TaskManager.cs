using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    public Text tasksText;
    public AudioSource audioSource;
    public AudioClip pencilSound;
    public float soundVolume = 0.8f;
    public float charDelay = 0.04f;
    [TextArea] public string task_GoDown = "?????????? ????";
    [TextArea] public string task_SolvePuzzle = "????????? ?????????? ???????";
    [TextArea] public string task_EnterRoom = "????? ? ???????";
    [TextArea] public string task_FindCode = "????? ??? ? ?????? ? ?????????";
    [TextArea] public string task_TakeCard = "????? ???????? ?? ?????";
    [TextArea] public string task_SwipeAlarm = "???????? ?????? — ????????? ???????";
    [TextArea] public string task_SwipeExit = "???????? ?????? ? ??????";

    private List<string> activeTasks = new List<string>();
    private List<string> completedTasks = new List<string>();
    private Coroutine typeCoroutine;
    private string lastDisplayedText = "";

    void Awake() { if (Instance == null) Instance = this; else { Destroy(gameObject); return; } }

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        AddTask(task_GoDown);
    }

    public void AddTask(string t)
    {
        if (string.IsNullOrEmpty(t) || activeTasks.Contains(t) || completedTasks.Contains(t)) return;
        activeTasks.Add(t);
        if (pencilSound != null) audioSource.PlayOneShot(pencilSound, soundVolume);
        Refresh();
    }

    public void CompleteTask(string t)
    {
        if (!activeTasks.Contains(t)) return;
        activeTasks.Remove(t); completedTasks.Add(t); Refresh();
    }

    void Refresh() { if (typeCoroutine != null) StopCoroutine(typeCoroutine); typeCoroutine = StartCoroutine(TypeText()); }

    string BuildText()
    {
        string r = "???????:\n\n";
        foreach (string t in activeTasks) r += "[ ] " + t + "\n";
        if (completedTasks.Count > 0) r += "\n";
        foreach (string t in completedTasks) r += "[+] " + t + "\n";
        return r;
    }

    IEnumerator TypeText()
    {
        if (tasksText == null) yield break;
        string full = BuildText();
        if (full.Length <= lastDisplayedText.Length) { tasksText.text = full; lastDisplayedText = full; yield break; }
        string newPart = full.Substring(lastDisplayedText.Length);
        string current = lastDisplayedText;
        foreach (char c in newPart) { current += c; tasksText.text = current; yield return new WaitForSeconds(charDelay); }
        lastDisplayedText = full;
    }
}