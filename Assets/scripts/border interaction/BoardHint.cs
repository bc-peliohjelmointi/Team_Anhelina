using UnityEngine;
using TMPro;

public class BoardHint : MonoBehaviour
{
    public GameObject hintUI;

    void Start()
    {
        hintUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hintUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hintUI.SetActive(false);
        }
    }
}