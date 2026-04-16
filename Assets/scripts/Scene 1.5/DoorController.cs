using UnityEngine;
using System.Collections;
// universal door script, moves between a closed and open position
// works for office door, alarm room door and the exit door
// set positions in inspector by placing door then copying localPosition
// Lock() stops door from opening even if you call OpenDoor()
public class DoorController : MonoBehaviour
{
    // record the localPosition of the door when it is closed
    public Vector3 closedLocalPosition;
    // record the localPosition of the door when it is fully open
    public Vector3 openLocalPosition;
    // how fast the door moves, higher = faster
    public float speed = 2f;
    // easing curve for smooth open/close animation
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    // set true only for doors that start open when scene loads
    public bool startsOpen = false;
    // dont set this manually, scripts control it via Lock() and Unlock()
    public bool isLocked = false;
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    // plays when player tries to open a locked door
    public AudioClip lockedSound;

    private bool isOpen;
    private bool isMoving;

    void Start()
    {
        isOpen = startsOpen;
        transform.localPosition = isOpen ? openLocalPosition : closedLocalPosition;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void OpenDoor()
    {
        if (isMoving || isOpen) return;
        if (isLocked)
        {
            if (lockedSound != null) audioSource.PlayOneShot(lockedSound);
            return;
        }
        StartCoroutine(Move(closedLocalPosition, openLocalPosition, openSound));
        isOpen = true;
    }

    public void CloseDoor()
    {
        if (isMoving || !isOpen) return;
        StartCoroutine(Move(openLocalPosition, closedLocalPosition, closeSound));
        isOpen = false;
    }

    // GameFlowManager calls these to block/unblock door
    public void Lock() => isLocked = true;
    public void Unlock() => isLocked = false;
    public bool IsOpen() => isOpen;

    IEnumerator Move(Vector3 from, Vector3 to, AudioClip sound)
    {
        isMoving = true;
        if (sound != null) audioSource.PlayOneShot(sound);
        float elapsed = 0f;
        float duration = 1f / speed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(from, to,
                moveCurve.Evaluate(Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        transform.localPosition = to;
        isMoving = false;
    }

    // green cube = open position, red cube = closed position, visible in scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            transform.parent != null ? transform.parent.TransformPoint(openLocalPosition) : openLocalPosition,
            Vector3.one * 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.parent != null ? transform.parent.TransformPoint(closedLocalPosition) : closedLocalPosition,
            Vector3.one * 0.3f);
    }
}