using UnityEngine;
using System.Collections;

public class BOSSInteract : MonoBehaviour
{
    [System.Serializable]
    public class AnimationVoices
    {
        public string animationName;
        public AudioClip line1;
        public AudioClip line2;
    }

    [Header("Animation + Voice pairs")]
    public AnimationVoices[] animationVoicePairs;

    [Header("Settings")]
    public string idleAnimation = "Idle standing mad";
    public float interactDistance = 3f;
    public float cooldown = 2.5f;
    public float linePause = 1.5f;

    [Header("Special Voice")]
    public AudioClip specialVoice;
    public float specialVoiceCooldown = 10f;

    [Header("UI")]
    public GameObject skipPromptUI;

    public Animator animator;
    private AudioSource audioSource;
    private Transform player;

    public bool isInteracting = false;
    public bool onCooldown = false;
    public bool onLine1 = false;
    public bool onLine2 = false;
    private bool specialVoiceOnCooldown = false;
    private AnimationVoices currentPair;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        player = GameObject.FindWithTag("Player").transform;

        if (skipPromptUI != null)
            skipPromptUI.SetActive(false);
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > interactDistance) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!isInteracting && !onCooldown)
                StartCoroutine(InteractRoutine());
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isInteracting) return;

            if (onLine1)
            {
                StopAllCoroutines();
                audioSource.Stop();
                onLine1 = false;
                StartCoroutine(PlayLine2Routine(currentPair));
            }
            else if (onLine2)
            {
                StopAllCoroutines();
                audioSource.Stop();
                onLine2 = false;

                if (!specialVoiceOnCooldown && specialVoice != null)
                {
                    audioSource.PlayOneShot(specialVoice);
                    specialVoiceOnCooldown = true;
                    Invoke(nameof(ResetSpecialVoice), specialVoiceCooldown);
                }

                animator.SetTrigger("GoIdle");
                StartCoroutine(ResetAfterSkip());
            }
        }
    }

    void ResetSpecialVoice()
    {
        specialVoiceOnCooldown = false;
    }

    IEnumerator ResetAfterSkip()
    {
        if (skipPromptUI != null) skipPromptUI.SetActive(false);
        isInteracting = false;

        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    IEnumerator InteractRoutine()
    {
        isInteracting = true;

        int index = Random.Range(0, animationVoicePairs.Length);
        currentPair = animationVoicePairs[index];

        animator.StopPlayback();
        animator.Play(currentPair.animationName, 0, 0f);

        onLine1 = true;
        if (skipPromptUI != null) skipPromptUI.SetActive(true);
        audioSource.PlayOneShot(currentPair.line1);
        yield return new WaitForSeconds(currentPair.line1.length);
        onLine1 = false;

        if (skipPromptUI != null) skipPromptUI.SetActive(false);
        yield return new WaitForSeconds(linePause);

        yield return StartCoroutine(PlayLine2Routine(currentPair));
    }

    IEnumerator PlayLine2Routine(AnimationVoices pair)
    {
        onLine2 = true;
        if (skipPromptUI != null) skipPromptUI.SetActive(true);
        audioSource.PlayOneShot(pair.line2);
        yield return new WaitForSeconds(pair.line2.length);
        onLine2 = false;

        if (skipPromptUI != null) skipPromptUI.SetActive(false);

        animator.SetTrigger("GoIdle");
        isInteracting = false;

        onCooldown = true;
        yield return new WaitForSeconds(cooldown);
        onCooldown = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}