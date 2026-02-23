using UnityEngine;

public class PSScreen : MonoBehaviour
{
    public Renderer screenRenderer;
    public Material onScreenMaterial;
    public Material offScreenMaterial;

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
    }

    public void TurnOn()
    {
        isOn = true;
        if (screenRenderer != null && onScreenMaterial != null)
        {
            screenRenderer.material = onScreenMaterial;
        }
    }

    public void TurnOff()
    {
        isOn = false;
        if (screenRenderer != null && offScreenMaterial != null)
        {
            screenRenderer.material = offScreenMaterial;
        }
    }

    public bool IsOn()
    {
        return isOn;
    }
}