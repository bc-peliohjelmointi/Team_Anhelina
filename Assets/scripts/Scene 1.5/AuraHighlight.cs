using UnityEngine;
// attach this to any object you want to glow when player looks at it
// works together with ObjectDragRayLevers which calls SetGlow(true/false)
public class AuraHighlight : MonoBehaviour
{
    [Header("Glow Settings")]
    // the color of the emission glow, white by default
    public Color glowColor = Color.white;
    // peak brightness of the glow
    public float glowIntensity = 1.5f;
    // speed of the breathing pulse animation
    public float glowSpeed = 3f;
    // if false, glow is static with no animation
    public bool usePulse = true;
    [Header("Outline Settings")]
    // optional outline effect instead of emission, usually leave this off
    public bool useOutline = false;
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 0.02f;

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] glowMaterials;
    private bool isGlowing = false;
    private float pulseTimer = 0f;

    void Awake()
    {
        // grab all renderers including children, works for multi-mesh objects
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        glowMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                // save the original so we can restore it later
                originalMaterials[i] = renderers[i].material;
                // make a copy with emission enabled for the glow version
                glowMaterials[i] = new Material(renderers[i].material);
                glowMaterials[i].EnableKeyword("_EMISSION");
            }
        }
    }

    void Update()
    {
        if (!isGlowing) return;

        if (usePulse)
        {
            // sine wave makes the glow breathe in and out smoothly
            pulseTimer += Time.deltaTime * glowSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
            float currentIntensity = Mathf.Lerp(glowIntensity * 0.4f, glowIntensity, pulse);
            for (int i = 0; i < glowMaterials.Length; i++)
            {
                if (glowMaterials[i] != null)
                    glowMaterials[i].SetColor("_EmissionColor", glowColor * currentIntensity);
            }
        }
    }

    // called by ObjectDragRayLevers when player starts or stops looking at this object
    public void SetGlow(bool on)
    {
        isGlowing = on;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            if (on)
            {
                // swap to glow material and set initial brightness
                renderers[i].material = glowMaterials[i];
                glowMaterials[i].SetColor("_EmissionColor", glowColor * glowIntensity);
                pulseTimer = 0f;
            }
            else
            {
                // put original material back
                renderers[i].material = originalMaterials[i];
            }
        }
    }

    public bool IsGlowing()
    {
        return isGlowing;
    }

    // make sure we restore original material if object gets disabled while glowing
    void OnDisable()
    {
        SetGlow(false);
    }
}