using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetMovement(float x, float z, bool shiftPressed)
    {
        bool isMoving = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;

        anim.SetBool("isMoving", isMoving);

        // chỉ chạy nhanh khi đang đi + shift
        bool isRunning = isMoving && shiftPressed;
        anim.SetBool("isRunning", isRunning);
    }
    public void SetJump(bool jump)
    {
        anim.SetBool("isJumping", jump);
    }
}
