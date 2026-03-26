using System.Collections;
using UnityEngine;

public class ScareEventLook : MonoBehaviour
{
    [Header("Monster")]
    public GameObject monsterPrefab;
    public float monsterVisibleTime = 4f;
    public float spawnDistance = 5f;      // дистанция спавна позади игрока

    [Header("Player Look")]
    public Transform playerTransform;
    public float lookSpeed = 3f;
    public float lookDuration = 3f;

    [Header("Player Control")]
    public MonoBehaviour playerController;

    [Header("Sound")]
    public AudioSource audioSource;
    public AudioClip scareSound;
    public AudioClip ambientSound;

    [Header("Post Processing Effects")]
    public UnityEngine.Rendering.Volume postProcessVolume;
    public float vignetteIntensityTarget = 0.6f;
    public float chromaticAberrationTarget = 1f;

    private bool hasFired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasFired) return;
        if (!other.CompareTag("Player")) return;

        hasFired = true;
        StartCoroutine(DoScare());
    }

    private IEnumerator DoScare()
    {
        if (playerController != null)
            playerController.enabled = false;

        // Считаем позицию позади игрока в момент срабатывания
        Vector3 spawnPosition = playerTransform.position + (-playerTransform.forward * spawnDistance);
        spawnPosition.y = playerTransform.position.y; // на одном уровне с игроком

        // Монстр смотрит на игрока сразу при спавне
        Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;
        Quaternion monsterRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0f, 90f, 0f);

        GameObject spawnedMonster = Instantiate(monsterPrefab, spawnPosition, monsterRotation);

        if (audioSource != null)
        {
            if (ambientSound != null)
                audioSource.PlayOneShot(ambientSound);
            if (scareSound != null)
                audioSource.PlayOneShot(scareSound);
        }

        StartCoroutine(FadePostProcessing(true));

        // Поворачиваем игрока на -90 по Y
        yield return StartCoroutine(RotatePlayer(-180f));

        yield return new WaitForSeconds(lookDuration);

        StartCoroutine(FadePostProcessing(false));

        Destroy(spawnedMonster);

        if (playerController != null)
            playerController.enabled = true;
    }

    private IEnumerator RotatePlayer(float yAngle)
    {
        float elapsed = 0f;
        Quaternion startRotation = playerTransform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, yAngle, 0f);

        while (elapsed < 1f / lookSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * lookSpeed);
            t = Mathf.SmoothStep(0f, 1f, t);

            playerTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        // Гарантируем точное финальное значение
        playerTransform.rotation = targetRotation;
    }

    private IEnumerator FadePostProcessing(bool fadeIn)
    {
        if (postProcessVolume == null) yield break;

        UnityEngine.Rendering.Universal.Vignette vignette;
        UnityEngine.Rendering.Universal.ChromaticAberration chromatic;

        postProcessVolume.profile.TryGet(out vignette);
        postProcessVolume.profile.TryGet(out chromatic);

        float duration = 1.5f;
        float elapsed = 0f;

        float vignetteStart = fadeIn ? 0f : vignetteIntensityTarget;
        float vignetteEnd = fadeIn ? vignetteIntensityTarget : 0f;
        float chromaticStart = fadeIn ? 0f : chromaticAberrationTarget;
        float chromaticEnd = fadeIn ? chromaticAberrationTarget : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(vignetteStart, vignetteEnd, t);
            if (chromatic != null)
                chromatic.intensity.value = Mathf.Lerp(chromaticStart, chromaticEnd, t);

            yield return null;
        }
    }
}