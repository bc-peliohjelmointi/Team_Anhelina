using UnityEngine;
// attach to any object you want to glow when player looks at it
// ObjectDragRay calls SetGlow(true/false)
public class AuraHighlight : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.white;
    public float glowIntensity = 1.5f;
    public float glowSpeed = 3f;
    public bool usePulse = true;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Material[][] glowMaterials;
    private bool isGlowing = false;
    private float pulseTimer = 0f;

    void Awake()
    {
        // grab every renderer in this object and all children
        renderers = GetComponentsInChildren<Renderer>(true);

        originalMaterials = new Material[renderers.Length][];
        glowMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            // renderer may have multiple material slots, handle all of them
            Material[] origSlots = renderers[i].materials;
            originalMaterials[i] = origSlots;

            Material[] glowSlots = new Material[origSlots.Length];
            for (int j = 0; j < origSlots.Length; j++)
            {
                glowSlots[j] = new Material(origSlots[j]);
                glowSlots[j].EnableKeyword("_EMISSION");
                // needed for URP
                glowSlots[j].SetFloat("_EmissionEnabled", 1f);
            }
            glowMaterials[i] = glowSlots;
        }
    }

    void Update()
    {
        if (!isGlowing || !usePulse) return;

        pulseTimer += Time.deltaTime * glowSpeed;
        float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
        float currentIntensity = Mathf.Lerp(glowIntensity * 0.4f, glowIntensity, pulse);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || glowMaterials[i] == null) continue;
            foreach (Material mat in glowMaterials[i])
            {
                if (mat != null)
                    mat.SetColor("_EmissionColor", glowColor * currentIntensity);
            }
        }
    }

    public void SetGlow(bool on)
    {
        isGlowing = on;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (on)
            {
                renderers[i].materials = glowMaterials[i];
                // set initial intensity right away so there is no one-frame flicker
                foreach (Material mat in glowMaterials[i])
                {
                    if (mat != null)
                        mat.SetColor("_EmissionColor", glowColor * glowIntensity);
                }
                pulseTimer = 0f;
            }
            else
            {
                renderers[i].materials = originalMaterials[i];
            }
        }
    }

    public bool IsGlowing() => isGlowing;

    void OnDisable()
    {
        SetGlow(false);
    }
}