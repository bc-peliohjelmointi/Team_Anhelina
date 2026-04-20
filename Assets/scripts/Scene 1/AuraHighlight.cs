using UnityEngine;

// attach this to any object you want to glow when player looks at it
// works together with ObjectDragRay which calls SetGlow(true/false)
public class AuraHighlight : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.white;
    public float glowIntensity = 1.5f;
    public float glowSpeed = 3f;         // speed of pulse effect
    public bool usePulse = true;         // if false glow is static, no animation

    [Header("Outline Settings")]
    public bool useOutline = false;      // optional outline instead of emission glow
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 0.02f;

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] glowMaterials;
    private bool isGlowing = false;
    private float pulseTimer = 0f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        glowMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                // save original material
                originalMaterials[i] = renderers[i].material;

                // create glow copy
                glowMaterials[i] = new Material(renderers[i].material);

                // enable emission on the glow material
                glowMaterials[i].EnableKeyword("_EMISSION");
            }
        }
    }

    void Update()
    {
        if (!isGlowing) return;

        if (usePulse)
        {
            // sine wave pulse so glow breathes in and out
            pulseTimer += Time.deltaTime * glowSpeed;
            float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f; // 0 to 1
            float currentIntensity = Mathf.Lerp(glowIntensity * 0.4f, glowIntensity, pulse);

            for (int i = 0; i < glowMaterials.Length; i++)
            {
                if (glowMaterials[i] != null)
                {
                    glowMaterials[i].SetColor("_EmissionColor", glowColor * currentIntensity);
                }
            }
        }
    }

    // called by ObjectDragRay when player looks at this object
    public void SetGlow(bool on)
    {
        isGlowing = on;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (on)
            {
                // switch to glow material
                renderers[i].material = glowMaterials[i];
                glowMaterials[i].SetColor("_EmissionColor", glowColor * glowIntensity);
                pulseTimer = 0f;
            }
            else
            {
                // restore original material
                renderers[i].material = originalMaterials[i];
            }
        }
    }

    public bool IsGlowing()
    {
        return isGlowing;
    }

    // safety cleanup in case object gets destroyed while glowing
    void OnDisable()
    {
        SetGlow(false);
    }
}