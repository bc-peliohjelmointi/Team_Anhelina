using UnityEngine;
using System.Collections;

public class LockMovementTimer : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public float lockDuration = 40f;

    private float originalWalkSpeed;
    private float originalRunSpeed;
    private float originalJumpHeight;

    void Start()
    {
        originalWalkSpeed = playerMovement.walkSpeed;
        originalRunSpeed = playerMovement.runSpeed;
        originalJumpHeight = playerMovement.jumpHeight;

        StartCoroutine(LockRoutine());
    }

    IEnumerator LockRoutine()
    {
        // lock movement + jump
        playerMovement.walkSpeed = 0f;
        playerMovement.runSpeed = 0f;
        playerMovement.jumpHeight = 0f;

        yield return new WaitForSeconds(lockDuration);

        // restore
        playerMovement.walkSpeed = originalWalkSpeed;
        playerMovement.runSpeed = originalRunSpeed;
        playerMovement.jumpHeight = originalJumpHeight;
    }
}