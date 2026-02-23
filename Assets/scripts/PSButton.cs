using System.Collections;
using UnityEngine;
using UnityEngine.Device;

public class PSButton : MonoBehaviour
{
    public PSScreen psScreen;
    public Light indicatorLight;
    public Color offColor = Color.red;
    public Color onColor = Color.gray;
    public float cooldownTime = 5f;

    public AudioSource buttonClickSound;

    private bool isOn = false;
    private bool canPress = true;

    void Start()
    {
        if (indicatorLight != null)
        {
            indicatorLight.color = offColor;
        }
    }

    public void Press()
    {
        if (!canPress) return;

        isOn = !isOn;

        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }

        if (indicatorLight != null)
        {
            indicatorLight.color = isOn ? onColor : offColor;
        }

        if (psScreen != null)
        {
            if (isOn)
            {
                psScreen.TurnOn();
            }
            else
            {
                psScreen.TurnOff();
            }
        }

        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        canPress = false;
        yield return new WaitForSeconds(cooldownTime);
        canPress = true;
    }

    public bool IsOn()
    {
        return isOn;
    }
}