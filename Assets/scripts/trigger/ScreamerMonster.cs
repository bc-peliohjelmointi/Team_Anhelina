using System.Collections;
using UnityEngine;

public class ScreamerMonster : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float rotationSpeed = 10f;
    public Transform target; // игрок

    [Header("Audio")]
    public AudioClip screamerSound;   // звук скриммера при появлении
    public AudioClip jumpscareMusic;  // фоновая музыка нагнетания
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Effects")]
    public GameObject jumpscareImageUI; // UI изображение (черный экран с лицом)
    public float jumpscareDistance = 1.5f; // дистанция до срабатывания смерти
    public float activationDelay = 0.3f;   // задержка перед движением

    [Header("Screen Shake")]
    public float shakeIntensity = 0.3f;
    public float shakeDuration = 0.5f;

    private AudioSource audioSource;
    private bool isActive = false;
    private bool playerIsDead = false;
    private Camera mainCamera;
    private Vector3 originalCamPos;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D звук — слышно везде
        audioSource.volume = volume;
        mainCamera = Camera.main;
    }

    // Вызывается из триггера
    public void Activate(Transform playerTransform)
    {
        if (isActive) return;
        target = playerTransform;
        gameObject.SetActive(true);
        StartCoroutine(ActivateSequence());
    }

    IEnumerator ActivateSequence()
    {
        // Воспроизвести звук скриммера сразу
        if (screamerSound != null)
            audioSource.PlayOneShot(screamerSound, volume);

        if (jumpscareMusic != null)
        {
            audioSource.clip = jumpscareMusic;
            audioSource.loop = true;
            audioSource.Play();
        }

        yield return new WaitForSeconds(activationDelay);
        isActive = true;
    }

    void Update()
    {
        if (!isActive || playerIsDead || target == null) return;

        // Поворот к игроку
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotationSpeed);
        }

        // Движение к игроку
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Проверка дистанции до смерти
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= jumpscareDistance)
        {
            TriggerDeath();
        }
    }

    void TriggerDeath()
    {
        if (playerIsDead) return;
        playerIsDead = true;
        isActive = false;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Тряска камеры
        StartCoroutine(ShakeCamera());

        // Показать UI джампскейра
        if (jumpscareImageUI != null)
            jumpscareImageUI.SetActive(true);

        // Заморозить движение игрока
        var playerController = target.GetComponent<CharacterController>();
        if (playerController != null)
            playerController.enabled = false;

        var rb = target.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        yield return new WaitForSeconds(2f);

        // Скрыть джампскейр и показать экран смерти или перезагрузить сцену
        if (jumpscareImageUI != null)
            jumpscareImageUI.SetActive(false);

        // Загрузить сцену заново (или вызвать свой GameManager)
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    IEnumerator ShakeCamera()
    {
        if (mainCamera == null) yield break;
        originalCamPos = mainCamera.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            mainCamera.transform.localPosition = new Vector3(
                originalCamPos.x + x,
                originalCamPos.y + y,
                originalCamPos.z
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCamPos;
    }
}