using UnityEngine;
using System.Collections;

public class TVPowerEffect : MonoBehaviour
{
    public Renderer screenRenderer;
    public Material noiseShaderMaterial;
    public Material onScreenMaterial;
    public Material offScreenMaterial;
    public float transitionDuration = 0.5f;

    private bool isTransitioning = false;

    void Start()
    {
        if (screenRenderer == null)
        {
            screenRenderer = GetComponent<Renderer>();
        }

        if (screenRenderer != null && offScreenMaterial != null)
        {
            screenRenderer.material = offScreenMaterial;
        }
    }

    public void TurnOn()
    {
        if (isTransitioning) return;
        StopAllCoroutines();
        StartCoroutine(TransitionToOn());
    }

    public void TurnOff()
    {
        if (isTransitioning) return;
        StopAllCoroutines();
        StartCoroutine(TransitionToOff());
    }

    IEnumerator TransitionToOn()
    {
        isTransitioning = true;

        if (noiseShaderMaterial != null && screenRenderer != null)
        {
            screenRenderer.material = noiseShaderMaterial;
        }

        yield return new WaitForSeconds(transitionDuration);

        if (onScreenMaterial != null && screenRenderer != null)
        {
            screenRenderer.material = onScreenMaterial;
        }

        isTransitioning = false;
    }

    IEnumerator TransitionToOff()
    {
        isTransitioning = true;

        if (noiseShaderMaterial != null && screenRenderer != null)
        {
            screenRenderer.material = noiseShaderMaterial;
        }

        yield return new WaitForSeconds(transitionDuration);

        if (offScreenMaterial != null && screenRenderer != null)
        {
            screenRenderer.material = offScreenMaterial;
        }

        isTransitioning = false;
    }
}