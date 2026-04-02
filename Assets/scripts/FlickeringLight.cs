using System.Collections;
using System.Drawing;
using UnityEngine;

public class FlickeringLight : MonoBehaviour
{
    [Header("Light")]
    public Light lightSource;

    [Header("Flicker Settings")]
    public float minOnTime = 0.05f;
    public float maxOnTime = 0.3f;
    public float minOffTime = 0.02f;
    public float maxOffTime = 0.15f;
    public float minIntensity = 0f;
    public float maxIntensity = 1f;

    [Header("Flicker Pattern")]
    public bool randomFlicker = true;
    public float flickerChance = 0.7f;
    public float normalLightDuration = 2f;
    public float flickerBurstDuration = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip lightOnSound;
    public AudioClip lightOffSound;
    public AudioClip flickerSound;
    public float soundVolume = 0.5f;
    public bool playSoundOnFlicker = true;

    private float originalIntensity;
    private bool isFlickering = false;

    void Start()
    {
        if (lightSource == null)
        {
            lightSource = GetComponent<Light>();
        }

        if (lightSource != null)
        {
            originalIntensity = lightSource.intensity;
            maxIntensity = originalIntensity;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.volume = soundVolume;

        StartCoroutine(FlickerRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            if (randomFlicker && Random.value > flickerChance)
            {
                lightSource.intensity = maxIntensity;
                yield return new WaitForSeconds(normalLightDuration);
            }
            else
            {
                float burstEndTime = Time.time + flickerBurstDuration;

                while (Time.time < burstEndTime)
                {
                    yield return StartCoroutine(FlickerOnce());
                }
            }
        }
    }

    IEnumerator FlickerOnce()
    {
        float onTime = Random.Range(minOnTime, maxOnTime);
        float offTime = Random.Range(minOffTime, maxOffTime);
        float intensity = Random.Range(minIntensity, maxIntensity);

        lightSource.intensity = intensity;
        lightSource.enabled = true;

        if (playSoundOnFlicker && lightOnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lightOnSound, soundVolume);
        }

        yield return new WaitForSeconds(onTime);

        lightSource.intensity = 0f;
        lightSource.enabled = false;

        if (playSoundOnFlicker && lightOffSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lightOffSound, soundVolume * 0.3f);
        }

        yield return new WaitForSeconds(offTime);
    }

    public void StartFlicker()
    {
        if (!isFlickering)
        {
            isFlickering = true;
            StartCoroutine(FlickerRoutine());
        }
    }

    public void StopFlicker()
    {
        isFlickering = false;
        StopAllCoroutines();
        lightSource.intensity = maxIntensity;
        lightSource.enabled = true;
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}
