using UnityEngine;
using TMPro;

public class InteractionHint : MonoBehaviour
{
    public static InteractionHint instance; // Staattinen viittaus ó k‰ytet‰‰n mist‰ tahansa koodissa
    public TextMeshProUGUI hintText;        // Tekstikentt‰, johon vihjeteksti n‰ytet‰‰n

    void Awake()
    {
        // Tallennetaan t‰m‰ objekti staattiseen muuttujaan
        instance = this;

        // Piilotetaan vihjeobjekti heti pelin alussa
        gameObject.SetActive(false);
    }

    // N‰ytet‰‰n vihje annetulla viestill‰
    public void Show(string message)
    {
        gameObject.SetActive(true);
        hintText.text = message;
    }

    // Piilotetaan vihje
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}