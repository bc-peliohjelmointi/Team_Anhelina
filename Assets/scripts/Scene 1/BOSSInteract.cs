using UnityEngine;
using System.Collections;

public class BOSSInteract : MonoBehaviour
{
    public Animator animator;
    public string[] interactAnimations;
    public string idleAnimation = "Idle standing mad";
    public float interactDistance = 3f;
    public float cooldown = 2.5f;

    private Transform player;
    public bool isInteracting = false;
    public bool onCooldown = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (isInteracting || onCooldown) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= interactDistance && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(InteractRoutine());
        }
    }

    IEnumerator InteractRoutine()
    {
        isInteracting = true;

        string anim = interactAnimations[Random.Range(0, interactAnimations.Length)];
        animator.CrossFade(anim, 0.3f);

        yield return new WaitForSeconds(0.35f);
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.CrossFade(idleAnimation, 0.3f);
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