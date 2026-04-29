using System.Collections;
using UnityEngine;

public class MSpawn : MonoBehaviour
{
    [Header("Monster")]
    public GameObject monster;

    [Header("Player Rotation")]
    public Transform playerTransform;
    public float rotateDuration = 0.5f;

    [Header("Voice Line")]
    public AudioClip voiceClip;
    public float voiceDelay = 0.2f; // задержка перед озвучкой

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerTransform == null)
                playerTransform = other.transform;

            StartCoroutine(SpawnSequence());
        }
    }

    IEnumerator SpawnSequence()
    {
        // 1. Активируем монстра
        monster.SetActive(true);

        // 2. Поворачиваем игрока в сторону монстра
        yield return StartCoroutine(RotatePlayerToMonster());

        // 3. Небольшая пауза перед озвучкой
        yield return new WaitForSeconds(voiceDelay);

        // 4. Играем озвучку
        if (voiceClip != null)
            audioSource.PlayOneShot(voiceClip);
    }

    IEnumerator RotatePlayerToMonster()
    {
        if (playerTransform == null) yield break;

        float elapsed = 0f;
        Quaternion startRot = playerTransform.rotation;

        Vector3 dir = monster.transform.position - playerTransform.position;
        dir.y = 0f;

        if (dir == Vector3.zero) yield break;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        while (elapsed < rotateDuration)
        {
            elapsed += Time.deltaTime;
            playerTransform.rotation = Quaternion.Slerp(startRot, targetRot, elapsed / rotateDuration);
            yield return null;
        }

        playerTransform.rotation = targetRot;
    }
}