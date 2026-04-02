using UnityEngine;
using System.Collections;
public class AuraHighlight : MonoBehaviour
{
    [Header("Renderers")]
    public Renderer[] renderers;
    [Header("Glow Settings")]
    public Color glowColor = new Color(0f, 1f, 1f, 1f);
    public float glowIntensity = 1.5f;
    public float fadeSpeed = 8f;

    [Header("Pulse")]
    public bool pulse = true;
    public float pulseSpeed = 2f;
    public float pulseMin = 0.8f;
    public float pulseMax = 1.8f;

    private bool isGlowing = false;
    private float currentIntensity = 0f;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private Material[][] originalMaterials;
    private Material[][] instanceMaterials;
    private bool initialized = false;

    void Start()
    {
        if (renderers == null || renderers.Length == 0)
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null) renderers = new Renderer[] { r };
        }

        originalMaterials = new Material[renderers.Length][];
        instanceMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            originalMaterials[i] = renderers[i].sharedMaterials;
            Material[] inst = new Material[renderers[i].sharedMaterials.Length];
            for (int j = 0; j < inst.Length; j++)
                inst[j] = new Material(renderers[i].sharedMaterials[j]);
            instanceMaterials[i] = inst;
        }

        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        float target = isGlowing ? glowIntensity : 0f;
        currentIntensity = Mathf.Lerp(currentIntensity, target, Time.deltaTime * fadeSpeed);

        float intensity = currentIntensity;
        if (isGlowing && pulse)
        {
            float p = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            intensity = Mathf.Lerp(pulseMin, pulseMax, p) * (currentIntensity / glowIntensity);
        }

        Color emission = glowColor * intensity;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || instanceMaterials[i] == null) continue;

            renderers[i].materials = instanceMaterials[i];

            foreach (Material mat in instanceMaterials[i])
            {
                if (mat == null) continue;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor(EmissionColor, emission);
            }
        }
    }

    public void SetGlow(bool enable)
    {
        isGlowing = enable;
        if (!enable && Mathf.Approximately(currentIntensity, 0f))
            RestoreOriginalMaterials();
    }

    void RestoreOriginalMaterials()
    {
        if (!initialized) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || originalMaterials[i] == null) continue;
            renderers[i].sharedMaterials = originalMaterials[i];
        }
    }

    void OnDestroy()
    {
        if (!initialized) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (instanceMaterials[i] == null) continue;
            foreach (Material mat in instanceMaterials[i])
                if (mat != null) Destroy(mat);
        }
    }
}