using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    public Rigidbody rb;
    public float pushForce = 5f;
    public bool canBePushed = true;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (rb.mass < 1f)
        {
            rb.mass = 1f;
        }
    }

    public void Push(Vector3 direction, float force)
    {
        if (canBePushed && rb != null)
        {
            rb.AddForce(direction * force * pushForce, ForceMode.Impulse);
        }
    }
}