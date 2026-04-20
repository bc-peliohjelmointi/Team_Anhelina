using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ImpactSound : MonoBehaviour
{
    [Header("Sound")]
    public AudioClip impactClip;
    public float minVelocityToPlay = 1.5f;
    public float volumeMultiplier = 1f;

    private AudioSource audioSource;
    private bool hasPlayedOnce = false; // play once only

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (impactClip == null) return;
        if (hasPlayedOnce) return; // already played, skip

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed >= minVelocityToPlay)
        {
            float vol = Mathf.Clamp01(impactSpeed / 10f) * volumeMultiplier;
            audioSource.PlayOneShot(impactClip, vol);
            hasPlayedOnce = true; // lock after first play
        }
    }
}