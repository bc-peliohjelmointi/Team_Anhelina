using UnityEngine;
using System.Collections;
public class DoorController : MonoBehaviour
{
    public Vector3 closedLocalPosition;
    public Vector3 openLocalPosition;
    public float speed = 2f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool startsOpen = false;
    public bool isLocked = false;
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
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
        if (isLocked) { if (lockedSound != null) audioSource.PlayOneShot(lockedSound); return; }
        StartCoroutine(Move(closedLocalPosition, openLocalPosition, openSound));
        isOpen = true;
    }

    public void CloseDoor()
    {
        if (isMoving || !isOpen) return;
        StartCoroutine(Move(openLocalPosition, closedLocalPosition, closeSound));
        isOpen = false;
    }

    public void Lock() => isLocked = true;
    public void Unlock() => isLocked = false;
    public bool IsOpen() => isOpen;

    IEnumerator Move(Vector3 from, Vector3 to, AudioClip sound)
    {
        isMoving = true;
        if (sound != null) audioSource.PlayOneShot(sound);
        float elapsed = 0f, duration = 1f / speed;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(from, to, moveCurve.Evaluate(Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        transform.localPosition = to;
        isMoving = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.parent != null ? transform.parent.TransformPoint(openLocalPosition) : openLocalPosition, Vector3.one * 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.parent != null ? transform.parent.TransformPoint(closedLocalPosition) : closedLocalPosition, Vector3.one * 0.3f);
    }
}