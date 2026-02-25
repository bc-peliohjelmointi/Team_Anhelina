using UnityEngine;
using System.Collections;

public class PSScreen : MonoBehaviour
{
    public Renderer screenRenderer;
    public Material glitchMaterial;
    public Material offScreenMaterial;

    [Header("Menu Timing")]
    public float glitchShowDelay = 0f;
    public float menuShowDelay = 1f;
    public bool useTypingEffect = true;
    public float typingSpeed = 0.05f;

    [Header("Menu Objects")]
    public PSMenuObject[] menuObjects = new PSMenuObject[3];
    public PSMenuObject[] checkmarkObjects = new PSMenuObject[3];

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

        InitializeMenus();
    }

    void InitializeMenus()
    {
        foreach (var menu in menuObjects)
        {
            if (menu != null)
            {
                menu.gameObject.SetActive(false);
            }
        }

        foreach (var check in checkmarkObjects)
        {
            if (check != null)
            {
                check.gameObject.SetActive(false);
            }
        }
    }

    public void TurnOn()
    {
        if (isOn) return;

        isOn = true;
        StopAllCoroutines();
        StartCoroutine(TurnOnSequence());
    }

    public void TurnOff()
    {
        if (!isOn) return;

        isOn = false;
        StopAllCoroutines();

        HideAllMenus();

        if (screenRenderer != null && offScreenMaterial != null)
        {
            screenRenderer.material = offScreenMaterial;
        }
    }

    IEnumerator TurnOnSequence()
    {
        yield return new WaitForSeconds(glitchShowDelay);

        if (screenRenderer != null && glitchMaterial != null && isOn)
        {
            screenRenderer.material = glitchMaterial;
        }

        yield return new WaitForSeconds(menuShowDelay);

        if (isOn)
        {
            if (useTypingEffect)
            {
                StartCoroutine(TypeMenus());
            }
            else
            {
                ShowAllMenus();
            }
        }
    }

    IEnumerator TypeMenus()
    {
        for (int i = 0; i < menuObjects.Length; i++)
        {
            if (menuObjects[i] != null)
            {
                menuObjects[i].gameObject.SetActive(true);
                yield return StartCoroutine(menuObjects[i].TypeText(typingSpeed));
            }
        }
    }

    void ShowAllMenus()
    {
        foreach (var menu in menuObjects)
        {
            if (menu != null)
            {
                menu.gameObject.SetActive(true);
                menu.ShowFullText();
            }
        }
    }

    void HideAllMenus()
    {
        foreach (var menu in menuObjects)
        {
            if (menu != null)
            {
                menu.gameObject.SetActive(false);
                menu.ResetText();
            }
        }

        foreach (var check in checkmarkObjects)
        {
            if (check != null)
            {
                check.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateSelection(int index)
    {
        for (int i = 0; i < menuObjects.Length; i++)
        {
            if (menuObjects[i] != null)
            {
                if (i == index)
                {
                    menuObjects[i].Highlight();
                }
                else
                {
                    menuObjects[i].Unhighlight();
                }
            }
        }
    }

    public void ShowCheckmark(int index)
    {
        if (index >= 0 && index < checkmarkObjects.Length && checkmarkObjects[index] != null)
        {
            checkmarkObjects[index].gameObject.SetActive(true);
        }
    }

    public void HideCheckmark(int index)
    {
        if (index >= 0 && index < checkmarkObjects.Length && checkmarkObjects[index] != null)
        {
            checkmarkObjects[index].gameObject.SetActive(false);
        }
    }

    public bool IsOn()
    {
        return isOn;
    }
}