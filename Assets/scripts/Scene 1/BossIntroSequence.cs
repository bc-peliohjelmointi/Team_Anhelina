using UnityEngine;
using System.Collections;

public class BossIntroSequence : MonoBehaviour
{
    public Animator animator;
    public Transform target;
    public float walkSpeed = 5f;
    public float rotateDuration = 1f;
    public bool IsFinished = false;

    private bool hasStarted = false;
    private float idleTimer = 0f;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!hasStarted && animator.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {
            hasStarted = true;
            StartCoroutine(WalkRoutine());
        }

        // Delay 2 giây sau khi idle chạy mới IsFinished
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

    IEnumerator WalkRoutine()
    {
        Debug.Log("=== BossIntro: Start RotateBy -170");
        yield return StartCoroutine(RotateBy(-170f));
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