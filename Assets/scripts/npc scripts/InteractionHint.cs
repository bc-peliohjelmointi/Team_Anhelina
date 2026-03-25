using UnityEngine;
using TMPro;

public class InteractionHint : MonoBehaviour
{
    public static InteractionHint instance;
    public TextMeshProUGUI hintText;

    void Awake()
    {
        instance = this;
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        gameObject.SetActive(true);
        hintText.text = message;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}