using UnityEngine;

// attach to any object you want to highlight when player looks at it
// switches shader to Standard/Lit with emission - guaranteed to work
// FIXED FOR BUILD: uses material.EnableKeyword instead of Shader.Find
// works in both editor and standalone build
public class AuraHighlight : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.white;
    public float glowIntensity = 1.5f;
    public float glowSpeed = 3f;
    public bool usePulse = true;

    [Header("Debug")]
    public bool debugMode = false;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
    // instanced copies of original materials with emission enabled
    private Material[][] glowMaterials;
    private bool isGlowing = false;
    private float pulseTimer = 0f;

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        if (debugMode)
            Debug.Log($"[AuraHighlight] {gameObject.name} — found {renderers.Length} renderers");

        originalMaterials = new Material[renderers.Length][];
        glowMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            // sharedMaterials = original assets, materials = instanced copies
            originalMaterials[i] = renderers[i].sharedMaterials;

            Material[] orig = renderers[i].sharedMaterials;
            Material[] glow = new Material[orig.Length];

            for (int j = 0; j < orig.Length; j++)
            {
                if (orig[j] == null) continue;

                // create an instanced copy of the SAME material
                // this keeps the original shader so it is guaranteed present in build
                glow[j] = new Material(orig[j]);

                // enable emission on the copy
                glow[j].EnableKeyword("_EMISSION");

                // URP uses _EmissionColor, Built-in also uses _EmissionColor
                // both paths covered here
                if (glow[j].HasProperty("_EmissionColor"))
                    glow[j].SetColor("_EmissionColor", glowColor * glowIntensity);

                // URP also needs globalIlluminationFlags cleared to see emission
                glow[j].globalIlluminationFlags =
                    MaterialGlobalIlluminationFlags.RealtimeEmissive;

                if (debugMode)
                    Debug.Log($"[AuraHighlight] glow mat created for renderer[{i}] mat[{j}] " +
                              $"shader={orig[j].shader.name}");
            }

            glowMaterials[i] = glow;
        }
    }

    void Update()
    {
        if (!isGlowing || !usePulse) return;

        pulseTimer += Time.deltaTime * glowSpeed;
        float pulse = (Mathf.Sin(pulseTimer) + 1f) / 2f;
        float intensity = Mathf.Lerp(glowIntensity * 0.4f, glowIntensity, pulse);

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || glowMaterials[i] == null) continue;
            foreach (Material mat in glowMaterials[i])
            {
                if (mat != null && mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", glowColor * intensity);
            }
        }
    }

    public void SetGlow(bool on)
    {
        isGlowing = on;
        pulseTimer = 0f;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (on)
            {
                // switch to instanced glow materials
                renderers[i].materials = glowMaterials[i];
                foreach (Material mat in glowMaterials[i])
                {
                    if (mat != null && mat.HasProperty("_EmissionColor"))
                        mat.SetColor("_EmissionColor", glowColor * glowIntensity);
                }
            }
            else
            {
                // restore original shared materials
                renderers[i].sharedMaterials = originalMaterials[i];
            }

            if (debugMode)
                Debug.Log($"[AuraHighlight] SetGlow({on}) — {renderers[i].gameObject.name}");
        }
    }

    public bool IsGlowing() => isGlowing;

    void OnDisable() => SetGlow(false);

    void OnDestroy()
    {
        // clean up instanced materials to avoid memory leaks
        if (glowMaterials == null) return;
        foreach (Material[] arr in glowMaterials)
        {
            if (arr == null) continue;
            foreach (Material mat in arr)
                if (mat != null) Destroy(mat);
        }
    }
}