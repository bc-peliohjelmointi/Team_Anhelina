using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

[System.Serializable]
public class DialogueSubtitle
{
    [TextArea] public string text;
    public float startTime;
    public float endTime;
}

public class NPCInteraction : MonoBehaviour
{
    [Header("Mission Settings")]
    public int missionID;
    public MissionSystem missionSystem;

    [Header("Dialogue Settings")]
    public AudioClip dialogueAudio;
    public DialogueSubtitle[] subtitles;
    public TextMeshProUGUI subtitleText;
    public float fadeSpeed = 3f;

    [Header("Trigger Mode")]
    public bool autoPlayOnEnter = false;

    [Header("Animation Settings")]
    public Animator npcAnimator;

    // Названия параметров/состояний в Animator
    private static readonly string ANIM_SIT_TALK = "SitTalk";   // Bool или Trigger
    private static readonly string ANIM_STAND_UP = "StandUp";   // Trigger
    private static readonly string ANIM_WALK = "Walk";      // Bool
    private static readonly string ANIM_STAND_TALK = "StandTalk"; // Bool
    private static readonly string ANIM_SIT_IDLE = "SitIdle";   // Bool (обычное сидение без разговора)

    [Header("NPC Movement (Return to Seat)")]
    public NavMeshAgent navAgent;          // NavMeshAgent на NPC
    public Transform seatTransform;        // Точка, где NPC сидело изначально
    public float returnStopDistance = 0.3f;// Расстояние, при котором считаем что дошли

    private AudioSource audioSource;
    private bool playerNear = false;
    private bool isTalking = false;
    private bool isReturning = false;
    private CharacterController playerController;

    // Сохраняем начальный поворот NPC
    private Quaternion initialRotation;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;

        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }

        // Запоминаем начальный поворот
        initialRotation = transform.rotation;

        // Отключаем NavMeshAgent пока не нужен
        if (navAgent != null)
            navAgent.enabled = false;

        // Начальная анимация — сидит и говорит (или просто сидит)
        SetAnimationState(AnimState.SitTalk);
    }

    // -------------------------------------------------------
    // Перечисление всех состояний анимации
    // -------------------------------------------------------
    enum AnimState { SitTalk, StandUp, Walk, StandTalk, SitIdle }

    void SetAnimationState(AnimState state)
    {
        if (npcAnimator == null) return;

        // Сначала сбрасываем все Bool-параметры
        npcAnimator.SetBool(ANIM_SIT_TALK, false);
        npcAnimator.SetBool(ANIM_WALK, false);
        npcAnimator.SetBool(ANIM_STAND_TALK, false);
        npcAnimator.SetBool(ANIM_SIT_IDLE, false);

        switch (state)
        {
            case AnimState.SitTalk:
                npcAnimator.SetBool(ANIM_SIT_TALK, true);
                break;

            case AnimState.StandUp:
                // Trigger — одноразовое срабатывание, переход к Walk/Stand
                npcAnimator.SetTrigger(ANIM_STAND_UP);
                break;

            case AnimState.Walk:
                npcAnimator.SetBool(ANIM_WALK, true);
                break;

            case AnimState.StandTalk:
                npcAnimator.SetBool(ANIM_STAND_TALK, true);
                break;

            case AnimState.SitIdle:
                npcAnimator.SetBool(ANIM_SIT_IDLE, true);
                break;
        }
    }

    // -------------------------------------------------------
    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isTalking && !autoPlayOnEnter && !isReturning)
        {
            if (missionSystem == null || missionSystem.GetCurrentMission() == missionID)
            {
                StartCoroutine(PlayDialogue());
                if (missionSystem != null)
                    missionSystem.CompleteMission(missionID);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = true;
        playerController = other.GetComponent<CharacterController>();

        if (!isTalking && !autoPlayOnEnter && !isReturning)
            InteractionHint.instance.Show("Press E to talk");

        if (autoPlayOnEnter && !isTalking && !isReturning)
            StartCoroutine(PlayDialogue());
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerNear = false;
        playerController = null;
        InteractionHint.instance.Hide();
        StopDialogue();
    }

    void SetPlayerMovement(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }

    void StopDialogue()
    {
        if (!isTalking) return;
        StopAllCoroutines();
        audioSource.Stop();
        isTalking = false;
        SetPlayerMovement(true);

        if (subtitleText != null)
        {
            subtitleText.text = "";
            Color c = subtitleText.color;
            subtitleText.color = new Color(c.r, c.g, c.b, 0f);
        }

        // Возвращаем NPC на место
        StartCoroutine(ReturnToSeat());
    }

    // -------------------------------------------------------
    // Основной диалог с тайм-кодами анимации
    // -------------------------------------------------------
    IEnumerator PlayDialogue()
    {
        isTalking = true;
        InteractionHint.instance.Hide();
        SetPlayerMovement(false);

        // --- Фаза 1: 0–27 сек — сидит и говорит ---
        SetAnimationState(AnimState.SitTalk);

        if (dialogueAudio != null)
        {
            audioSource.clip = dialogueAudio;
            audioSource.Play();
        }

        int currentSubtitle = -1;
        bool stoodUp = false;
        bool startedWalk = false;
        bool startedStandTalk = false;

        while (audioSource.isPlaying)
        {
            float elapsed = audioSource.time;

            // --- Переключение анимаций по времени ---

            // 27 сек — встаёт (Trigger)
            if (!stoodUp && elapsed >= 27f)
            {
                stoodUp = true;
                SetAnimationState(AnimState.StandUp);
            }

            // 29 сек — идёт (небольшая задержка после вставания, подбери сам)
            if (!startedWalk && elapsed >= 29f)
            {
                startedWalk = true;
                SetAnimationState(AnimState.Walk);
            }

            // 33 сек — стоит и говорит
            if (!startedStandTalk && elapsed >= 33f)
            {
                startedStandTalk = true;
                SetAnimationState(AnimState.StandTalk);
            }

            // --- Субтитры ---
            int activeLine = -1;
            for (int i = 0; i < subtitles.Length; i++)
            {
                if (elapsed >= subtitles[i].startTime && elapsed < subtitles[i].endTime)
                {
                    activeLine = i;
                    break;
                }
            }

            if (activeLine != currentSubtitle)
            {
                currentSubtitle = activeLine;
                StopCoroutine("FadeText");
                if (currentSubtitle >= 0)
                    StartCoroutine(FadeText(subtitles[currentSubtitle].text, true));
                else
                    StartCoroutine(FadeText("", false));
            }

            yield return null;
        }

        // Ääni on loppunut
        StartCoroutine(FadeText("", false));
        SetPlayerMovement(true);
        isTalking = false;

        // После конца диалога — NPC возвращается на место
        StartCoroutine(ReturnToSeat());

        if (playerNear)
            InteractionHint.instance.Show("Press E to talk");
    }

    // -------------------------------------------------------
    // Возврат NPC на исходную позицию
    // -------------------------------------------------------
    IEnumerator ReturnToSeat()
    {
        if (seatTransform == null || navAgent == null) yield break;
        if (isReturning) yield break;

        isReturning = true;

        // Включаем NavMeshAgent и запускаем ходьбу
        navAgent.enabled = true;
        navAgent.isStopped = false;
        navAgent.SetDestination(seatTransform.position);
        SetAnimationState(AnimState.Walk);

        // Ждём пока NPC дойдёт до места
        while (true)
        {
            float dist = Vector3.Distance(transform.position, seatTransform.position);
            if (dist <= returnStopDistance)
                break;

            // Дополнительная проверка через NavMeshAgent
            if (!navAgent.pathPending && navAgent.remainingDistance <= returnStopDistance)
                break;

            yield return null;
        }

        // Пришли — останавливаем агента
        navAgent.isStopped = true;
        navAgent.enabled = false;

        // Возвращаем исходный поворот плавно
        yield return StartCoroutine(RotateToInitial());

        // Садимся обратно — небольшая задержка перед анимацией "сесть"
        // Если у тебя есть анимация "садится" — добавь Trigger здесь
        // npcAnimator.SetTrigger("SitDown");
        // yield return new WaitForSeconds(1.5f); // время анимации приседания

        // Финальное состояние — сидит и говорит (или SitIdle если молчит)
        SetAnimationState(AnimState.SitTalk);

        isReturning = false;
    }

    // Плавный поворот к начальному вращению
    IEnumerator RotateToInitial()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Quaternion from = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(from, initialRotation, elapsed / duration);
            yield return null;
        }

        transform.rotation = initialRotation;
    }

    // -------------------------------------------------------
    IEnumerator FadeText(string text, bool show)
    {
        if (subtitleText == null) yield break;
        Color c = subtitleText.color;

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed * 2f;
            c.a = Mathf.Max(c.a, 0f);
            subtitleText.color = c;
            yield return null;
        }

        subtitleText.text = text;
        if (!show) yield break;

        while (c.a < 1f)
        {
            c.a += Time.deltaTime * fadeSpeed;
            c.a = Mathf.Min(c.a, 1f);
            subtitleText.color = c;
            yield return null;
        }
    }
}