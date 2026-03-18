using UnityEngine;
using System.Collections;

public class MainDoorLever : MonoBehaviour
{
    [Header("Control Panel")]
    public MainControlPanel controlPanel;

    [Header("Door")]
    public Transform door;
    public Vector3 closedPosition;
    public Vector3 openPosition;
    public float doorSpeed = 1f;
    public AnimationCurve doorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Lever")]
    public Transform leverHandle;
    public Vector3 downRotation = new Vector3(45, 0, 0);
    public Vector3 upRotation = new Vector3(-45, 0, 0);
    public float leverSpeed = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip leverSound;
    public AudioClip doorSound;
    public AudioClip errorSound;
    public float soundVolume = 0.5f;

    private bool isDoorOpen = false;
    private bool isMoving = false;
    private bool leverUp = false;
    private Quaternion leverTargetRotation;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        leverTargetRotation = Quaternion.Euler(downRotation);
        if (leverHandle != null)
        {
            leverHandle.localRotation = leverTargetRotation;
        }

        if (door != null)
        {
            door.localPosition = closedPosition;
        }
    }

    void Update()
    {
        if (leverHandle != null)
        {
            leverHandle.localRotation = Quaternion.Lerp(
                leverHandle.localRotation,
                leverTargetRotation,
                Time.deltaTime * leverSpeed
            );
        }
    }

    public void PullLever()
    {
        if (isMoving) return;

        if (controlPanel != null && controlPanel.CanOpenDoor())
        {
            if (!isDoorOpen)
            {
                leverUp = true;
                leverTargetRotation = Quaternion.Euler(upRotation);
                StartCoroutine(OpenDoor());

                if (leverSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(leverSound, soundVolume);
                }
            }
        }
        else
        {
            if (errorSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(errorSound, soundVolume);
            }
        }
    }

    IEnumerator OpenDoor()
    {
        if (door == null) yield break;

        isMoving = true;

        if (doorSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(doorSound, soundVolume);
        }

        Vector3 startPos = door.localPosition;
        float elapsed = 0f;
        float duration = 1f / doorSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curvedT = doorCurve.Evaluate(t);

            door.localPosition = Vector3.Lerp(startPos, openPosition, curvedT);

            yield return null;
        }

        door.localPosition = openPosition;
        isDoorOpen = true;
        isMoving = false;
    }
}