using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Outline outline; // UI outline effect

    void Start()
    {
        outline.enabled = false; // start hidden
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.enabled = true; // show outline on hover
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.enabled = false; // hide outline when leave
    }
}