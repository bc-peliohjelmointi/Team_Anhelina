using UnityEngine;
using System.Collections;

public class BossIntroSequence : MonoBehaviour
{
    public Animator animator;
    public Transform target;
    public float walkSpeed = 2f;
    public float rotateDuration = 1f;
    public float finalRotationY = -450.217f; // góc Y muốn nhìn về khi đến nơi

    private bool isWalking = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isWalking && animator.GetCurrentAnimatorStateInfo(0).IsName("walk"))
        {
            isWalking = true;
            StartCoroutine(WalkRoutine());
        }
    }

    IEnumerator WalkRoutine()
    {
        yield return StartCoroutine(RotateBy(-170f));

        while (Vector3.Distance(transform.position, target.position) > 0.5f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(target.position.x, transform.position.y, target.position.z),
                walkSpeed * Time.deltaTime
            );
            yield return null;
        }

        yield return StartCoroutine(RotateTo(finalRotationY));

        animator.CrossFade("idle standing mad", 0.3f);
        isWalking = false;
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

    IEnumerator RotateTo(float targetYDegrees)
    {
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(0, targetYDegrees, 0);
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