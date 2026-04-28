using UnityEngine;
using System.Collections;

public class BossIntroSequence : MonoBehaviour
{
    public Animator animator;
    public string[] introAnimations;
    public string idleAnimation = "Idle standing mad";
    public float delay = 0f;
    public float crossFadeDuration = 0.3f;

    IEnumerator Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        yield return new WaitForSeconds(delay);

        foreach (string animName in introAnimations)
        {
            animator.CrossFade(animName, crossFadeDuration);

            // chờ đến khi state đúng tên mới chạy
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).IsName(animName));

            // chờ animation đó chạy xong
            yield return new WaitUntil(() =>
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        }

        animator.CrossFade(idleAnimation, crossFadeDuration);
    }
}