using UnityEngine;
using System.Collections;

// physical button that toggles the TV on and off
// has a cooldown so player cant spam it
public class TVButton : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public Light indicatorLight;
    public Color offColor = Color.red;
    public Color onColor = Color.gray;
    public float cooldownTime = 5f;         // seconds before button can be pressed again
    public AudioSource buttonClickSound;
    private bool isOn = false;
    private bool canPress = true;
    private bool wasEverTurnedOn = false;   // tracks if TV was ever turned on - used elsewhere for puzzle logic

    void Start()
    {
        if (indicatorLight != null)
        {
            indicatorLight.color = offColor; // start red (off)
        }
    }

    public void Press()
    {
        if (!canPress) return;

        isOn = !isOn;

        if (isOn)
        {
            wasEverTurnedOn = true; // set this once and never reset
        }

        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }

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

    public bool WasEverTurnedOn()
    {
        return wasEverTurnedOn;
    }
}