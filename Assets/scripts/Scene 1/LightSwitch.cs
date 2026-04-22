using UnityEngine;
// attach this to the lever object
// ObjectDragRay calls Toggle() when player clicks on it with left mouse button
// set the on and off positions by moving lever in scene and copying localPosition
public class LightSwitch : MonoBehaviour
{
    [Header("Light")]
    // the light source this switch controls
    public Light controlledLight;
    [Header("Switch Positions")]
    // local position when switch is in OFF state
    public Vector3 offPosition = Vector3.zero;
    // local position when switch is in ON state
    public Vector3 onPosition = Vector3.zero;
    // local euler rotation when switch is OFF
    public Vector3 offRotation = new Vector3(30f, 0f, 0f);
    // local euler rotation when switch is ON
    public Vector3 onRotation = new Vector3(-30f, 0f, 0f);
    // how fast lever animates between positions, higher = snappier
    public float animationSpeed = 8f;

    [Header("State")]
    // starting state when scene loads
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
        // snap to initial state without animation
        ApplyState(true);
    }

    void Update()
    {
        // smoothly animate lever toward target position and rotation every frame
        transform.localPosition = Vector3.Lerp(
            transform.localPosition, targetPosition, Time.deltaTime * animationSpeed);
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
    }

    // called by ObjectDragRay when player clicks on this object
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

        // turn light on or off
        if (controlledLight != null)
            controlledLight.enabled = isOn;

        // if immediate skip animation and snap directly
        if (immediate)
        {
            transform.localPosition = targetPosition;
            transform.localRotation = targetRotation;
        }
    }

    // helper methods in case other scripts need to force a specific state
    public void TurnOn() { if (!isOn) Toggle(); }
    public void TurnOff() { if (isOn) Toggle(); }
}