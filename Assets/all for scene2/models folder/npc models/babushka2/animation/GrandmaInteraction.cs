using System.Collections;
using UnityEngine;

public class GrandmaInteraction : MonoBehaviour
{
    public Animator animator;

    public float animationStartDelay = 19f; // задержка перед началом анимаций

    public float standupDelay = 1f;
    public float standDelay = 1f;
    public float walkDelay = 2f;
    public float sitDelay = 2f;

    private bool playerInRange = false;
    private bool isInteracting = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            StartCoroutine(InteractionSequence());
        }
    }

    IEnumerator InteractionSequence()
    {
        isInteracting = true;

        // Ждём 19 секунд
        yield return new WaitForSeconds(animationStartDelay);

        animator.SetTrigger("StandUp");
        yield return new WaitForSeconds(standupDelay);

        animator.SetTrigger("Stand");
        yield return new WaitForSeconds(standDelay);

        animator.SetTrigger("Walk");
        yield return new WaitForSeconds(walkDelay);

        animator.SetTrigger("SitDown");
        yield return new WaitForSeconds(sitDelay);

        isInteracting = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}