using UnityEngine;

public class TVJumpscare : MonoBehaviour
{
    [Header("References")]
    public GameObject tvScreen;
    public Light buttonLight;
    public AudioSource audioSource;
    public GameObject ghost;

    [Header("Settings")]
    public float delay = 1.5f;
    public float ghostDuration = 7f;

    private bool hasTriggered = false;

    void OnMouseDown()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        tvScreen.SetActive(false);
        buttonLight.enabled = false;
        Invoke(nameof(PlaySound), delay);
    }

    void PlaySound()
    {
        audioSource.Play();
        ghost.SetActive(true);
        Invoke(nameof(HideGhost), ghostDuration);
    }

    void HideGhost()
    {
        ghost.SetActive(false);
    }
}