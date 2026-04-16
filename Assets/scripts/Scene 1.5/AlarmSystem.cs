using UnityEngine;
using System.Collections;
// controls the flashing red alarm lights and siren sound
// only the lights you put in alarmLights array will flash
// uses instanced materials so other objects with same material are not affected
// started by GameFlowManager when player enters the alarm room
public class AlarmSystem : MonoBehaviour
{
    // only drag in the lights that should flash red, leave others out
    public Light[] alarmLights;
    public Color alarmColor = Color.red;
    // how many flashes per second, 2 means on 0.5s off 0.5s
    public float flashSpeed = 2f;
    // renderers of the lamp meshes that should glow red during alarm
    public Renderer[] alarmRenderers;
    public string emissionProperty = "_EmissionColor";
    // the audio source that plays the siren
    public AudioSource alarmAudioSource;
    // should be a looping audio clip
    public AudioClip alarmSound;
    public float alarmVolume = 0.8f;
    private bool isActive = false;
    private Coroutine flashCoroutine;
    // instanced materials so we dont mess up shared assets
    private Material[] rendererMaterials;

    void Start()
    {
        if (alarmAudioSource == null)
            alarmAudioSource = gameObject.AddComponent<AudioSource>();
        alarmAudioSource.playOnAwake = false;
        alarmAudioSource.loop = true;

        // create instanced materials only if we have renderers
        if (alarmRenderers != null && alarmRenderers.Length > 0)
        {
            rendererMaterials = new Material[alarmRenderers.Length];
            for (int i = 0; i < alarmRenderers.Length; i++)
                if (alarmRenderers[i] != null)
                    rendererMaterials[i] = alarmRenderers[i].material;
        }
        else
        {
            rendererMaterials = new Material[0];
        }

        SetLights(false);
    }

    // called by GameFlowManager when player enters alarm room
    public void StartAlarm()
    {
        if (isActive) return;
        isActive = true;
        if (alarmSound != null)
        {
            alarmAudioSource.clip = alarmSound;
            alarmAudioSource.volume = alarmVolume;
            alarmAudioSource.Play();
        }
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashLights());
    }

    // called by KeyCardReader when player swipes card at alarm reader
    public void StopAlarm()
    {
        if (!isActive) return;
        isActive = false;
        alarmAudioSource.Stop();
        if (flashCoroutine != null) { StopCoroutine(flashCoroutine); flashCoroutine = null; }
        SetLights(false);
    }

    public bool IsActive() => isActive;

    IEnumerator FlashLights()
    {
        while (true)
        {
            SetLights(true);
            yield return new WaitForSeconds(1f / flashSpeed);
            SetLights(false);
            yield return new WaitForSeconds(1f / flashSpeed);
        }
    }

    void SetLights(bool on)
    {
        if (alarmLights != null)
        {
            foreach (Light l in alarmLights)
            {
                if (l == null) continue;
                l.enabled = on;
                l.color = alarmColor;
            }
        }
        if (rendererMaterials != null && alarmRenderers != null)
        {
            for (int i = 0; i < alarmRenderers.Length; i++)
            {
                if (i >= rendererMaterials.Length) break;
                if (alarmRenderers[i] == null || rendererMaterials[i] == null) continue;
                if (on)
                {
                    rendererMaterials[i].EnableKeyword("_EMISSION");
                    rendererMaterials[i].SetColor(emissionProperty, alarmColor * 3f);
                }
                else
                {
                    rendererMaterials[i].DisableKeyword("_EMISSION");
                    rendererMaterials[i].SetColor(emissionProperty, Color.black);
                }
            }
        }
    }
}