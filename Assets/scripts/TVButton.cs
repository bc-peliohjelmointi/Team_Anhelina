using UnityEngine;

public class TVButton : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public Light indicatorLight;
    public Color offColor = Color.red;
    public Color onColor = Color.gray;

    private bool isOn = false;

    void Start()
    {
        if (indicatorLight != null)
        {
            indicatorLight.color = offColor;
        }
    }

    public void Press()
    {
        isOn = !isOn;

        if (indicatorLight != null)
        {
            indicatorLight.color = isOn ? onColor : offColor;
        }

        if (tvEffect != null)
        {
            if (isOn)
            {
                tvEffect.TurnOn();
            }
            else
            {
                tvEffect.TurnOff();
            }
        }
    }
}