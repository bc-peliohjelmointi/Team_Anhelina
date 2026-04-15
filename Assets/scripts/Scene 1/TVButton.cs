using UnityEngine;
using System.Collections;

public class TVButton : MonoBehaviour
{
    public TVPowerEffect tvEffect;
    public Light indicatorLight;
    public Color offColor = Color.red;
    public Color onColor = Color.gray;
    public float cooldownTime = 5f;

    public AudioSource buttonClickSound;

    private bool isOn = false;
    private bool canPress = true;
    private bool wasEverTurnedOn = false;

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

        if (isOn)
        {
            wasEverTurnedOn = true;
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