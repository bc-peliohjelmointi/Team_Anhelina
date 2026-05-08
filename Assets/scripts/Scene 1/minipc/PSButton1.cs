using UnityEngine;
// ============================================================
// PSButton1.cs
// ============================================================
// Put this on the physical power button of the PC/monitor
// ObjectDragRay detects this via GetComponent<PSButton1>()
// and calls Press() when player clicks LMB on the button
//
// NOTE: renamed from PSButton to PSButton1 to avoid conflict
// with existing PSButton script in your project
//
// First press  -> PowerOn()  -> desktop activates, E prompt appears
// Second press -> PowerOff() -> PC turns off, desktop hides
// ============================================================
public class PSButton1 : MonoBehaviour
{
    [Header("References")]
    // drag the monitor/PC object with PSComputerSystem here
    public PSComputerSystem computerSystem;

    [Header("Button Animation")]
    // the physical button mesh transform that moves when pressed
    public Transform buttonTransform;
    // how far button moves inward when pressed (local Z axis)
    public float pressDepth = 0.005f;
    public float pressSpeed = 15f;

    [Header("LED Indicator")]
    // renderer of the small LED light on the button
    public Renderer ledRenderer;
    public Color offColor = Color.red;
    public Color onColor = Color.green;
    public string emissionProperty = "_EmissionColor";

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clickSound;
    public float soundVolume = 0.6f;

    private Vector3 restPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;

    void Start()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        if (buttonTransform != null)
        {
            restPosition = buttonTransform.localPosition;
            pressedPosition = restPosition + Vector3.back * pressDepth;
        }
        SetLED(false);
    }

    void Update()
    {
        if (buttonTransform == null) return;
        // smoothly animate button mesh to pressed or rest position
        Vector3 target = isPressed ? pressedPosition : restPosition;
        buttonTransform.localPosition = Vector3.Lerp(
            buttonTransform.localPosition, target, Time.deltaTime * pressSpeed);
    }

    // called by ObjectDragRay when player clicks LMB on this object
    public void Press()
    {
        if (computerSystem == null) return;
        if (clickSound != null) audioSource.PlayOneShot(clickSound, soundVolume);
        StartCoroutine(PressAnimation());
        if (!computerSystem.IsPoweredOn())
        {
            // PC is off - turn it on
            computerSystem.PowerOn();
            SetLED(true);
        }
        else
        {
            // PC is on - turn it off
            computerSystem.PowerOff();
            SetLED(false);
        }
    }

    System.Collections.IEnumerator PressAnimation()
    {
        isPressed = true;
        yield return new WaitForSeconds(0.15f);
        isPressed = false;
    }

    void SetLED(bool on)
    {
        if (ledRenderer == null) return;
        Material mat = ledRenderer.material;
        Color c = on ? onColor : offColor;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor(emissionProperty, c * 2f);
        mat.color = c;
    }
}