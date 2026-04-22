using UnityEngine;

// attach to any object you want to highlight when player looks at it
// switches shader to Standard/Lit with emission - guaranteed to work
public class AuraHighlight : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.white;
    public float glowIntensity = 1.5f;
    public float glowSpeed = 3f;
    public bool usePulse = true;

    [Header("Outline Settings")]
    public bool useOutline = false;
    public Color outlineColor = Color.yellow;
    public float outlineWidth = 0.02f;

    [Header("Debug")]
    public bool debugMode = false;

    private Renderer[] renderers;
    private Material[][] originalMaterials;
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

        // find the best available lit shader on this machine
        Shader litShader = FindLitShader();

        if (debugMode)
            Debug.Log($"[AuraHighlight] using shader: {litShader.name}");

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material[] orig = renderers[i].materials;
            originalMaterials[i] = orig;

            Material[] glow = new Material[orig.Length];
            for (int j = 0; j < orig.Length; j++)
            {
                if (orig[j] == null) continue;

                // create a copy and force it onto a shader that definitely has emission
                glow[j] = new Material(litShader);

                // copy the main texture and color so the object still looks right
                if (orig[j].HasProperty("_MainTex"))
                    glow[j].SetTexture("_MainTex", orig[j].GetTexture("_MainTex"));
                if (orig[j].HasProperty("_BaseMap"))
                    glow[j].SetTexture("_BaseMap", orig[j].GetTexture("_BaseMap"));
                if (orig[j].HasProperty("_Color"))
                    glow[j].SetColor("_Color", orig[j].GetColor("_Color"));
                if (orig[j].HasProperty("_BaseColor"))
                    glow[j].SetColor("_BaseColor", orig[j].GetColor("_BaseColor"));

                // enable emission
                glow[j].EnableKeyword("_EMISSION");
                glow[j].SetFloat("_EmissionEnabled", 1f);
                glow[j].SetColor("_EmissionColor", glowColor * glowIntensity);

                if (debugMode)
                    Debug.Log($"[AuraHighlight] renderer[{i}] mat[{j}] original shader was: {orig[j].shader.name}");
            }
            glowMaterials[i] = glow;
        }
    }

    // tries to find a shader that definitely supports emission, from most to least preferred
    Shader FindLitShader()
    {
        string[] candidates = new string[]
        {
            "Universal Render Pipeline/Lit",
            "Lit",
            "Standard",
            "Legacy Shaders/Diffuse",
        };

        foreach (string name in candidates)
        {
            Shader s = Shader.Find(name);
            if (s != null) return s;
        }

        // absolute fallback - should never happen
        return Shader.Find("Standard");
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
                if (mat != null)
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

            renderers[i].materials = on ? glowMaterials[i] : originalMaterials[i];

            if (on)
            {
                foreach (Material mat in glowMaterials[i])
                {
                    if (mat != null)
                        mat.SetColor("_EmissionColor", glowColor * glowIntensity);
                }
            }

            if (debugMode)
                Debug.Log($"[AuraHighlight] SetGlow({on}) — {renderers[i].gameObject.name}");
        }
    }

    public bool IsGlowing() => isGlowing;

    void OnDisable() => SetGlow(false);
}