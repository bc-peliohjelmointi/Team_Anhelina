using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float stoppingDistance = 1.0f;
    public float rotationSpeed = 10f;

    [Header("Audio Settings")]
    public AudioClip enemySound;
    public float hearingRange = 30f;
    public float startTime = 0f;
    public float endTime = 5f;

    private AudioSource audioSource;
    private bool playerInRange = false;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null && enemySound != null)
        {
            audioSource.clip = enemySound;
            audioSource.loop = false; // лупим вручную
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = hearingRange;
            audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        }
    }

    void Update()
    {
        if (player == null || audioSource == null) return;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        // Проверяем, в пределах ли Player hearingRange
        playerInRange = distance <= hearingRange;

        // Движение Enemy
        if (distance > stoppingDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

            if (direction != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
            }
        }

        // Управление звуком
        if (playerInRange)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.time = startTime;
                audioSource.Play();
            }

            // Loop сегмента
            if (audioSource.isPlaying && audioSource.time >= endTime)
            {
                audioSource.time = startTime;
                audioSource.Play();
            }

            // Регулировка громкости по расстоянию
            audioSource.volume = 1f - (distance / hearingRange);
        }
        else
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}
