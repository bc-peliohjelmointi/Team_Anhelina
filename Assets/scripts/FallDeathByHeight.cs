using UnityEngine;

public class FallDeathByHeight : MonoBehaviour
{
    public float deathHeight = 10f;
    private float startFallY;
    private bool isFalling;

    void Update()
    {
        if (!isFalling && GetComponent<CharacterController>().isGrounded)
        {
            startFallY = transform.position.y;
        }

        if (!GetComponent<CharacterController>().isGrounded)
        {
            isFalling = true;
        }

        if (isFalling && GetComponent<CharacterController>().isGrounded)
        {
            float fallDistance = startFallY - transform.position.y;

            if (fallDistance >= deathHeight)
            {
                Die();
            }

            isFalling = false;
        }
    }

    void Die()
    {
        Debug.Log("Игрок умер от высоты");
    }
}
