using UnityEngine;
using System.Collections;
public class MainDoorLever : MonoBehaviour
{
    [Header("References")]
    public MainControlPanel controlPanel;
    public GameFlowManager gameFlowManager;
    [Header("Door")]
    public Transform door;
    public Vector3 closedPosition;
    public Vector3 openPosition;
    public float doorSpeed = 1f;
    public AnimationCurve doorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Lever Handle")]
    public Transform leverHandle;
    public Vector3 downRotation = new Vector3(45f, 0f, 0f);
    public Vector3 upRotation = new Vector3(-45f, 0f, 0f);
    public float leverSpeed = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip leverSound;
    public AudioClip doorSound;
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
        if (leverHandle != null)
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation, leverTargetRotation, Time.deltaTime * leverSpeed);
    }

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
        if (gameFlowManager != null) gameFlowManager.OnPuzzleDoorOpened();
    }
}