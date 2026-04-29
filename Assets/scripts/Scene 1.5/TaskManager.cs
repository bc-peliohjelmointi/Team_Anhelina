using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
// manages the task list shown on the clipboard
// AddTask adds a new task with a typewriter typing sound effect
// CompleteTask moves it to the completed section below
// task strings must match EXACTLY when completing them
public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance { get; private set; }
    // Text component on the World Space Canvas on the clipboard
    public Text tasksText;
    // for playing the pencil scratch sound
    public AudioSource audioSource;
    public AudioClip pencilSound;
    public float soundVolume = 0.8f;
    // delay between each character in the typewriter effect
    public float charDelay = 0.04f;

    // you can change these task descriptions in inspector if you want
    [TextArea] public string task_GoDown = "Go down to the lower floor";
    [TextArea] public string task_SolvePuzzle = "Figure out the lever combination";
    [TextArea] public string task_EnterRoom = "Enter the room";
    [TextArea] public string task_FindCode = "Find the code and enter it at the computer";
    [TextArea] public string task_TakeCard = "Take the card from the drawer";
    [TextArea] public string task_SwipeAlarm = "Swipe card to disable alarm";
    [TextArea] public string task_SwipeExit = "Swipe card at the exit reader";

    private List<string> activeTasks = new List<string>();
    private List<string> completedTasks = new List<string>();
    private Coroutine typeCoroutine;
    private string lastDisplayedText = "";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        // first task appears automatically at scene start
        AddTask(task_GoDown);
    }

    public void AddTask(string t)
    {
        if (string.IsNullOrEmpty(t)) return;
        // skip duplicates
        if (activeTasks.Contains(t) || completedTasks.Contains(t)) return;
        activeTasks.Add(t);
        if (pencilSound != null) audioSource.PlayOneShot(pencilSound, soundVolume);
        Refresh();
    }

    public void CompleteTask(string t)
    {
        if (!activeTasks.Contains(t)) return;
        activeTasks.Remove(t);
        completedTasks.Add(t);
        Refresh();
    }

    void Refresh()
    {
        if (typeCoroutine != null) StopCoroutine(typeCoroutine);
        typeCoroutine = StartCoroutine(TypeText());
    }

    string BuildText()
    {
        string r = "TASKS:\n\n";
        foreach (string t in activeTasks) r += "[ ] " + t + "\n";
        if (completedTasks.Count > 0) r += "\n";
        foreach (string t in completedTasks) r += "[+] " + t + "\n";
        return r;
    }

    IEnumerator TypeText()
    {
        if (tasksText == null) yield break;
        string full = BuildText();
        // if text is shorter (task completed) just set it instantly
        if (full.Length <= lastDisplayedText.Length)
        {
            tasksText.text = full;
            lastDisplayedText = full;
            yield break;
        }
        // type only the new characters that were added
        string newPart = full.Substring(lastDisplayedText.Length);
        string current = lastDisplayedText;
        foreach (char c in newPart)
        {
            current += c;
            tasksText.text = current;
            yield return new WaitForSeconds(charDelay);
        }
        lastDisplayedText = full;
    }
}