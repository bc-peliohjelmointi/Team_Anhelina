using UnityEngine;

public class JumpscareSound : MonoBehaviour
{
    public AudioClip jumpscareClip;
    private AudioSource audioSource;
    private bool triggered = false;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = jumpscareClip;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!triggered && other.CompareTag("Player"))
        {
            audioSource.Play();
            triggered = true; 
        }
    }
}