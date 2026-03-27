using UnityEngine;
using System.Collections;
public class AlarmSystem : MonoBehaviour
{
    [Header("Lights — ???????? ?????? ?????? ?????")]
    public Light[] alarmLights;
    public Color alarmColor = Color.red;
    public float flashSpeed = 2f;
    [Header("Renderers ????????")]
    public Renderer[] alarmRenderers;
    public string emissionProperty = "_EmissionColor";

    [Header("Audio")]
    public AudioSource alarmAudioSource;
    public AudioClip alarmSound;
    public float alarmVolume = 0.8f;

    private bool isActive = false;
    private Coroutine flashCoroutine;
    private Material[] rendererMaterials;

    void Start()
    {
        if (alarmAudioSource == null) alarmAudioSource = gameObject.AddComponent<AudioSource>();
        alarmAudioSource.playOnAwake = false;
        alarmAudioSource.loop = true;

        rendererMaterials = new Material[alarmRenderers.Length];
        for (int i = 0; i < alarmRenderers.Length; i++)
            if (alarmRenderers[i] != null)
                rendererMaterials[i] = alarmRenderers[i].material;

        SetLights(false);
    }

    public void StartAlarm()
    {
        if (isActive) return;
        isActive = true;
        if (alarmSound != null) { alarmAudioSource.clip = alarmSound; alarmAudioSource.volume = alarmVolume; alarmAudioSource.Play(); }
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashLights());
    }

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
        foreach (Light l in alarmLights)
        {
            if (l == null) continue;
            l.enabled = on;
            l.color = alarmColor;
        }
        for (int i = 0; i < alarmRenderers.Length; i++)
        {
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