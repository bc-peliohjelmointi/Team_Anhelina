using UnityEngine;
using System.Collections;

public class PSScreen : MonoBehaviour
{
    public Renderer screenRenderer;
    public Material glitchMaterial;
    public Material offScreenMaterial;
    public PSScreenUI screenUI;
    public float menuShowDelay = 1f;

    private bool isOn = false;

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

        if (screenUI == null)
        {
            screenUI = gameObject.AddComponent<PSScreenUI>();
        }
    }

    public void TurnOn()
    {
        isOn = true;
        StopAllCoroutines();
        StartCoroutine(TurnOnSequence());
    }

    public void TurnOff()
    {
        isOn = false;
        StopAllCoroutines();

        if (screenUI != null)
        {
            screenUI.HideMenu();
        }

        if (screenRenderer != null && offScreenMaterial != null)
        {
            screenRenderer.material = offScreenMaterial;
        }
    }

    IEnumerator TurnOnSequence()
    {
        if (screenRenderer != null && glitchMaterial != null)
        {
            screenRenderer.material = glitchMaterial;
        }

        yield return new WaitForSeconds(menuShowDelay);

        if (screenUI != null && isOn)
        {
            screenUI.ShowMenu();
        }
    }

    public bool IsOn()
    {
        return isOn;
    }

    public PSScreenUI GetScreenUI()
    {
        return screenUI;
    }
}