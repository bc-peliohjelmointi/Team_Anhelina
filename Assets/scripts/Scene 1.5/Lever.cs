using UnityEngine;
// small lever with one indicator light (red or green)
// toggles up/down and tells its owner puzzle row to update light states
// only fill ONE of the ownerLevel fields - the row this lever belongs to
// leverRenderer is the lever handle mesh, lampRenderer is the little indicator bulb
public class Lever : MonoBehaviour
{
    // current state, false = down, true = up
    public bool isUp = false;
    // local euler rotation when lever is in down position
    public Vector3 downRotation = new Vector3(45f, 0f, 0f);
    // local euler rotation when lever is in up position
    public Vector3 upRotation = new Vector3(-45f, 0f, 0f);
    // local position when down, usually Vector3.zero
    public Vector3 downPosition = Vector3.zero;
    // local position when up, usually Vector3.zero too
    public Vector3 upPosition = Vector3.zero;
    // how fast lever animates between positions
    public float switchSpeed = 5f;
    // the Unity Light component attached to the indicator bulb
    public Light lampLight;
    // the mesh renderer of the indicator bulb
    public Renderer lampRenderer;
    public Color redColor = Color.red;
    public Color greenColor = Color.green;
    // emission property name, should match your shader
    public string emissionProperty = "_EmissionColor";

    public AudioSource audioSource;
    public AudioClip switchSound;
    public float soundVolume = 0.5f;

    // mesh renderer of the lever handle, used for outline highlight in panel mode
    public Renderer leverRenderer;
    public Color highlightColor = new Color(0f, 1f, 1f, 1f);
    public float outlineWidth = 0.015f;

    // only set the level this lever belongs to, leave others empty
    public PuzzleLevel1 ownerLevel1;
    public PuzzleLevel2 ownerLevel2;
    public PuzzleLevel3 ownerLevel3;

    private Quaternion targetRotation;
    private Vector3 targetPosition;
    private bool isLightGreen = false;
    private Material lampMaterial;
    private Material[] originalMaterials;
    private Material[] highlightMaterials;

    void Start()
    {
        if (lampRenderer != null) lampMaterial = lampRenderer.material;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;
        if (leverRenderer != null) originalMaterials = leverRenderer.materials;
        // apply initial state immediately without animation
        ApplyState(true);
    }

    void Update()
    {
        // smoothly animate toward target rotation and position every frame
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * switchSpeed);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * switchSpeed);
    }

    public void Toggle()
    {
        isUp = !isUp;
        ApplyState(false);
        if (switchSound != null) audioSource.PlayOneShot(switchSound, soundVolume);
        // notify owner row that a lever changed so it can update lights
        if (ownerLevel1 != null) ownerLevel1.OnLeverChanged();
        if (ownerLevel2 != null) ownerLevel2.OnLeverChanged();
        if (ownerLevel3 != null) ownerLevel3.OnLeverChanged();
    }

    // called by PuzzleLevel to set indicator color
    public void SetLightGreen(bool green)
    {
        isLightGreen = green;
        UpdateLight();
    }

    void ApplyState(bool immediate)
    {
        targetRotation = Quaternion.Euler(isUp ? upRotation : downRotation);
        targetPosition = isUp ? upPosition : downPosition;
        if (immediate)
        {
            transform.localRotation = targetRotation;
            transform.localPosition = targetPosition;
        }
        UpdateLight();
    }

    void UpdateLight()
    {
        Color c = isLightGreen ? greenColor : redColor;
        if (lampLight != null) lampLight.color = c;
        if (lampMaterial != null)
        {
            lampMaterial.EnableKeyword("_EMISSION");
            lampMaterial.SetColor(emissionProperty, c * 2f);
            lampMaterial.color = c;
        }
    }

    // PuzzlePanelInteraction calls this when player hovers cursor over this lever
    public void Highlight(bool enable)
    {
        if (leverRenderer == null || originalMaterials == null) return;
        if (enable) CreateHighlight(); else RemoveHighlight();
    }

    void CreateHighlight()
    {
        if (highlightMaterials != null) return;
        Shader s = Shader.Find("Custom/OutlineEdge");
        if (s == null) return;
        highlightMaterials = new Material[originalMaterials.Length + 1];
        for (int i = 0; i < originalMaterials.Length; i++) highlightMaterials[i] = originalMaterials[i];
        Material m = new Material(s);
        m.SetColor("_OutlineColor", highlightColor);
        m.SetFloat("_OutlineWidth", outlineWidth);
        highlightMaterials[highlightMaterials.Length - 1] = m;
        leverRenderer.materials = highlightMaterials;
    }

    void RemoveHighlight()
    {
        if (highlightMaterials == null) return;
        leverRenderer.materials = originalMaterials;
        if (highlightMaterials.Length > originalMaterials.Length)
            Destroy(highlightMaterials[highlightMaterials.Length - 1]);
        highlightMaterials = null;
    }

    void OnDestroy()
    {
        if (lampMaterial != null) Destroy(lampMaterial);
        RemoveHighlight();
    }
}