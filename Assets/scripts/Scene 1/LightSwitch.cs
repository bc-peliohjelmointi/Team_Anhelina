using UnityEngine;
// attach to the lever object
// ObjectDragRay calls Toggle() when player clicks on it
public class LightSwitch : MonoBehaviour
{
    [Header("Light")]
    public Light controlledLight;

    [Header("Switch Positions")]
    public Vector3 offPosition = Vector3.zero;
    public Vector3 onPosition = Vector3.zero;
    public Vector3 offRotation = new Vector3(30f, 0f, 0f);
    public Vector3 onRotation = new Vector3(-30f, 0f, 0f);
    public float animationSpeed = 8f;

    [Header("State")]
    public bool isOn = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip switchSound;
    public float soundVolume = 0.8f;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        ApplyState(true);
    }

    void Update()
    {
        // smoothly animate lever toward target each frame
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * animationSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
    }

    public void Toggle()
    {
        isOn = !isOn;
        ApplyState(false);
        if (switchSound != null)
            audioSource.PlayOneShot(switchSound, soundVolume);
    }

    void ApplyState(bool immediate)
    {
        targetPosition = isOn ? onPosition : offPosition;
        targetRotation = Quaternion.Euler(isOn ? onRotation : offRotation);

        if (controlledLight != null)
            controlledLight.enabled = isOn;

        if (immediate)
        {
            transform.localPosition = targetPosition;
            transform.localRotation = targetRotation;
        }
    }

    public void TurnOn() { if (!isOn) Toggle(); }
    public void TurnOff() { if (isOn) Toggle(); }
}