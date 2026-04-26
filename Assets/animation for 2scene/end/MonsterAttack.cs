using UnityEngine;

public class MonsterAttack : MonoBehaviour
{
    public Animator animator;
    private bool isAttacking = false;

    void OnTriggerEnter(Collider other)
    {
        if (isAttacking) return;

        if (other.CompareTag("Player"))
        {
            isAttacking = true;
            animator.SetTrigger("Attack");
        }
    }

    public void KillPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.Die();
        }
    }
}