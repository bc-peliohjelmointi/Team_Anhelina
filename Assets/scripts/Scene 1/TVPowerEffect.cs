using UnityEngine;
using System.Collections;

public class TVPowerEffect : MonoBehaviour
{
    public Renderer screenRenderer;
    public Material glitchMaterial;
    public Material offScreenMaterial;
    public float transitionDuration = 0.5f;

    public AudioSource turnOnSound;
    public AudioSource turnOffSound;

    private bool isOn = false;
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

        if (turnOnSound != null)
        {
            turnOnSound.Play();
        }

        if (glitchMaterial != null && screenRenderer != null)
        {
            screenRenderer.material = glitchMaterial;
        }

        yield return new WaitForSeconds(transitionDuration);

        isOn = true;
        isTransitioning = false;
    }

    IEnumerator TransitionToOff()
    {
        isTransitioning = true;

        if (turnOffSound != null)
        {
            turnOffSound.Play();
        }

        if (offScreenMaterial != null && screenRenderer != null)
        {
            screenRenderer.material = offScreenMaterial;
        }

        yield return new WaitForSeconds(transitionDuration);

        isOn = false;
        isTransitioning = false;
    }

    public bool IsOn()
    {
        return isOn;
    }

    public void ReturnToNoise()
    {
        if (isOn && glitchMaterial != null && screenRenderer != null)
        {
            screenRenderer.material = glitchMaterial;
        }
    }
}