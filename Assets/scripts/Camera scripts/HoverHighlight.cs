using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    public GameObject hoverText;

    void Start()
    {
        if (hoverText != null)
            hoverText.SetActive(false);
    }

    void OnMouseOver()
    {
        if (hoverText != null)
            hoverText.SetActive(true);
    }

    void OnMouseExit()
    {
        if (hoverText != null)
            hoverText.SetActive(false);
    }
}