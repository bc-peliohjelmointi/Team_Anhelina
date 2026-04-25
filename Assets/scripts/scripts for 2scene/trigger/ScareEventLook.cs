using System.Collections;
using UnityEngine;

public class ScareEventLook : MonoBehaviour
{
    [Header("Monster")]
    public GameObject monsterPrefab;       // Hirviön prefab, joka ilmestyy pelästyskohtauksessa
    public float monsterVisibleTime = 4f;  // Kuinka kauan hirviö on näkyvissä sekunteissa
    public float spawnDistance = 5f;       // Kuinka kaukana pelaajan takana hirviö ilmestyy

    [Header("Player Look")]
    public Transform playerTransform; // Pelaajan transform-komponentti
    public float lookSpeed = 3f;      // Kuinka nopeasti pelaaja kääntyy hirviötä kohti
    public float lookDuration = 3f;   // Kuinka kauan pelaaja katsoo hirviötä sekunteissa

    [Header("Player Control")]
    public MonoBehaviour playerController; // Pelaajan liikkumiskomponentti

    [Header("Sound")]
    public AudioSource audioSource;  // Äänikomponentti pelästysäänelle
    public AudioClip scareSound;     // Pelästysääni

    [Header("Post Processing Effects")]
    public UnityEngine.Rendering.Volume postProcessVolume;  // Post-prosessointitehoste
    public float vignetteIntensityTarget = 0.6f;            // Vinjettitehosteen tavoitevoimakkuus
    public float chromaticAberrationTarget = 1f;            // Kromaattisen aberraation tavoitevoimakkuus

    private bool hasFired = false; // Onko pelästyskohtaus jo lauennut

    private void OnTriggerEnter(Collider other)
    {
        // Jos pelästys on jo lauennut, ei tehdä mitään
        if (hasFired) return;

        // Käynnistetään pelästyskohtaus, kun pelaaja astuu alueelle
        if (!other.CompareTag("Player")) return;

        hasFired = true;
        StartCoroutine(DoScare());
    }

    private IEnumerator DoScare()
    {
        // Estetään pelaajan liikkuminen pelästyskohtauksen ajaksi
        if (playerController != null)
            playerController.enabled = false;

        // Lasketaan hirviön ilmestymispaikka pelaajan takana
        Vector3 spawnPosition = playerTransform.position + (-playerTransform.forward * spawnDistance);
        spawnPosition.y = playerTransform.position.y; // Pidetään hirviö samalla korkeudella

        // Lasketaan suunta hirviöstä pelaajaan päin
        Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;

        // Lasketaan hirviön suunta — +90 astetta korjaa mallin suunnan
        Quaternion monsterRotation = Quaternion.LookRotation(directionToPlayer) * Quaternion.Euler(0f, 0f, 0f);

        // Luodaan hirviö laskettuun sijaintiin
        GameObject spawnedMonster = Instantiate(monsterPrefab, spawnPosition, monsterRotation);

        // Toistetaan pelästysääni heti — soi samalla kun pelaaja kääntyy
        if (audioSource != null && scareSound != null)
            audioSource.PlayOneShot(scareSound);

        // Käynnistetään post-prosessointitehoste päälle
        StartCoroutine(FadePostProcessing(true));

        // Käännetään pelaaja 180 astetta katsomaan hirviötä
        yield return StartCoroutine(RotatePlayer(-180f));

        // Odotetaan, että pelaaja katselee hirviötä
        yield return new WaitForSeconds(lookDuration);

        // Häivytetään post-prosessointitehoste pois
        StartCoroutine(FadePostProcessing(false));

        // Poistetaan hirviö scenestä
        Destroy(spawnedMonster);

        // Palautetaan pelaajan liikkuminen
        if (playerController != null)
            playerController.enabled = true;
    }

    private IEnumerator RotatePlayer(float yAngle)
    {
        float elapsed = 0f;
        Quaternion startRotation = playerTransform.rotation;

        // Lasketaan pelaajan kohdesuunta
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, yAngle, 0f);

        // Pyöritetään pelaajaa tasaisesti kohdesuuntaan
        while (elapsed < 1f / lookSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * lookSpeed);

            // SmoothStep tekee kääntymisestä pehmeämmän alussa ja lopussa
            t = Mathf.SmoothStep(0f, 1f, t);
            playerTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            yield return null;
        }

        // Varmistetaan, että pelaaja on täsmälleen kohdesuunnassa
        playerTransform.rotation = targetRotation;
    }

    private IEnumerator FadePostProcessing(bool fadeIn)
    {
        // Jos post-prosessointia ei ole asetettu, ei tehdä mitään
        if (postProcessVolume == null) yield break;

        // Haetaan vinjetti- ja kromaattiset aberraatiokomponentit
        UnityEngine.Rendering.Universal.Vignette vignette;
        UnityEngine.Rendering.Universal.ChromaticAberration chromatic;
        postProcessVolume.profile.TryGet(out vignette);
        postProcessVolume.profile.TryGet(out chromatic);

        float duration = 1.5f; // Häivytyksen kesto sekunteissa
        float elapsed = 0f;

        // Asetetaan alku- ja loppuarvot sen mukaan, häivytetäänkö tehosteet päälle vai pois
        float vignetteStart = fadeIn ? 0f : vignetteIntensityTarget;
        float vignetteEnd = fadeIn ? vignetteIntensityTarget : 0f;
        float chromaticStart = fadeIn ? 0f : chromaticAberrationTarget;
        float chromaticEnd = fadeIn ? chromaticAberrationTarget : 0f;

        // Muutetaan tehosteiden voimakkuuksia tasaisesti
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Interpoloidaan vinjettitehosteen voimakkuus
            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(vignetteStart, vignetteEnd, t);

            // Interpoloidaan kromaattisen aberraation voimakkuus
            if (chromatic != null)
                chromatic.intensity.value = Mathf.Lerp(chromaticStart, chromaticEnd, t);

            yield return null;
        }
    }
}