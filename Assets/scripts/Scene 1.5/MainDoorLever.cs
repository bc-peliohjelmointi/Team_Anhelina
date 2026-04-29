using UnityEngine;
using System.Collections;
// the big lever that opens the office door after all 3 rows are solved
// checks MainControlPanel.CanOpenDoor() first
// if puzzle not done yet plays error sound instead
// after door opens notifies GameFlowManager to advance game state
public class MainDoorLever : MonoBehaviour
{
    // reference to the control panel to check if puzzle is done
    public MainControlPanel controlPanel;
    // needed to trigger next phase after door opens
    public GameFlowManager gameFlowManager;
    // the transform of the actual door mesh to move
    public Transform door;
    // record these from the door position in scene
    public Vector3 closedPosition;
    public Vector3 openPosition;
    public float doorSpeed = 1f;
    public AnimationCurve doorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // the handle part of the lever that animates
    public Transform leverHandle;
    public Vector3 downRotation = new Vector3(45f, 0f, 0f);
    public Vector3 upRotation = new Vector3(-45f, 0f, 0f);
    public float leverSpeed = 5f;

    public AudioSource audioSource;
    public AudioClip leverSound;
    public AudioClip doorSound;
    // plays when puzzle isnt done yet and player tries the lever
    public AudioClip errorSound;
    public float soundVolume = 0.5f;

    private bool isDoorOpen = false;
    private bool isMoving = false;
    private Quaternion leverTargetRotation;

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        leverTargetRotation = Quaternion.Euler(downRotation);
        if (leverHandle != null) leverHandle.localRotation = leverTargetRotation;
        if (door != null) door.localPosition = closedPosition;
    }

    void Update()
    {
        // animate lever handle smoothly
        if (leverHandle != null)
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation, leverTargetRotation, Time.deltaTime * leverSpeed);
    }

    // called by MainDoorLeverInteraction when player presses E
    public void PullLever()
    {
        if (isMoving) return;
        if (controlPanel != null && controlPanel.CanOpenDoor())
        {
            if (!isDoorOpen)
            {
                leverTargetRotation = Quaternion.Euler(upRotation);
                if (leverSound != null) audioSource.PlayOneShot(leverSound, soundVolume);
                StartCoroutine(OpenDoor());
            }
        }
        else
        {
            // puzzle isnt solved, just play the error sound
            if (errorSound != null) audioSource.PlayOneShot(errorSound, soundVolume);
        }
    }

    IEnumerator OpenDoor()
    {
        if (door == null) yield break;
        isMoving = true;
        if (doorSound != null) audioSource.PlayOneShot(doorSound, soundVolume);
        Vector3 startPos = door.localPosition;
        float elapsed = 0f;
        float duration = 1f / doorSpeed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            door.localPosition = Vector3.Lerp(startPos, openPosition,
                doorCurve.Evaluate(Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        door.localPosition = openPosition;
        isDoorOpen = true;
        isMoving = false;
        // tell game manager door is open so next phase can start
        if (gameFlowManager != null) gameFlowManager.OnPuzzleDoorOpened();
    }
}