using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim; // animator reference

    void Awake()
    {
        anim = GetComponent<Animator>(); // get Animator component
    }

    public void SetMovement(float x, float z, bool shiftPressed)
    {
        // check if player is moving
        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
        anim.SetBool("isMoving", isMoving);

        // run only if moving + shift pressed
        bool isRunning = isMoving && shiftPressed;
        anim.SetBool("isRunning", isRunning);
    }

    public void SetJump(bool jump)
    {
        anim.SetBool("isJumping", jump); // trigger jump animation
    }
}