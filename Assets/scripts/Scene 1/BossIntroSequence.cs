using UnityEngine;
using System.Collections;

public class BossIntroSequence : MonoBehaviour
{
    public Animator animator;
    public Transform target;
    public float walkSpeed = 5f;
    public float rotateDuration = 1f;
    public bool IsFinished = false;

    [Header("Intro Voices")]
    public AudioSource introVoice1;
    public AudioSource introVoice2;
    public AudioSource introVoice3;
    public float voice2Delay = 1.5f; 
    public float voice3Delay = 1.5f;

    [Header("Text Lines")]
    public GameObject line1;
    public float line1ShowAt = 0f;   
    public float line1Duration = 8f; 

    public GameObject line2;
    public float line2ShowAt = 9f;
    public float line2Duration = 5f;

    public GameObject line3;
    public float line3ShowAt = 15f;
    public float line3Duration = 5f;

    public GameObject line4;
    public float line4ShowAt = 21f;
    public float line4Duration = 5f;

    private bool hasStarted = false;
    private float idleTimer = 0f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();


        if (line1 != null) line1.SetActive(false);
        if (line2 != null) line2.SetActive(false);
        if (line3 != null) line3.SetActive(false);
        if (line4 != null) line4.SetActive(false);


        StartCoroutine(IntroVoiceRoutine());
    }

    void Update()
    {
        if (!hasStarted && animator.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {
            hasStarted = true;
            StartCoroutine(WalkRoutine());
        }

        // Delay 2 IsFinished
        if (hasStarted && !IsFinished && animator.GetCurrentAnimatorStateInfo(0).IsName("idle standing mad"))
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= 1.5f)
            {
                IsFinished = true;
                Debug.Log("=== BossIntro: IsFinished = true");
            }
        }
    }

    IEnumerator IntroVoiceRoutine()
    {

        StartCoroutine(ShowLine(line1, line1ShowAt, line1Duration));
        StartCoroutine(ShowLine(line2, line2ShowAt, line2Duration));
        StartCoroutine(ShowLine(line3, line3ShowAt, line3Duration));
        StartCoroutine(ShowLine(line4, line4ShowAt, line4Duration));
        // play voice 1
        if (introVoice1 != null)
        {
            introVoice1.Play();
            yield return new WaitForSeconds(introVoice1.clip.length + voice2Delay);
        }

        // play voice 2
        if (introVoice2 != null)
        {
            introVoice2.Play();
            yield return new WaitForSeconds(introVoice2.clip.length + voice3Delay);
        }

        // play voice 3
        if (introVoice3 != null)
            introVoice3.Play();




        yield return null;
    }
    IEnumerator ShowLine(GameObject lineObj, float showAt, float duration)
    {
        if (lineObj == null) yield break;


        yield return new WaitForSeconds(showAt);
        lineObj.SetActive(true);


        yield return new WaitForSeconds(duration);
        lineObj.SetActive(false);
    }
    IEnumerator WalkRoutine()
    {
        Debug.Log("=== BossIntro: Start RotateBy -170");
        yield return StartCoroutine(RotateBy(-168f));
        Debug.Log("=== BossIntro: Rotate done, start walking. Distance = " + Vector3.Distance(transform.position, target.position));

        while (Vector3.Distance(transform.position, target.position) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(target.position.x, transform.position.y, target.position.z),
                walkSpeed * Time.deltaTime
            );
            yield return null;
        }

        Debug.Log("=== BossIntro: Reached target!");
        yield return StartCoroutine(RotateBy(170f));
        Debug.Log("=== BossIntro: Second rotate done");

        animator.CrossFade("idle standing mad", 0.3f);
    }

    IEnumerator RotateBy(float degrees)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = transform.rotation * Quaternion.Euler(0, degrees, 0);
        float timer = 0f;

        while (timer < rotateDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, timer / rotateDuration);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        transform.rotation = endRot;
    }
}