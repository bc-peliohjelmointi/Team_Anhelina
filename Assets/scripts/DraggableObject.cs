using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    public Rigidbody rb;
    public float pushForce = 5f;
    public bool canBePushed = true;
    public bool canBeGrabbed = true;
    public int frameNumber = -1;

    [Header("Physics Settings")]
    public float objectMass = 0.1f;
    public float airResistance = 1.5f;
    public bool freezeRotation = false;
    public bool isPaper = false;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (isPaper)
        {
            rb.mass = objectMass;
            rb.linearDamping = airResistance;
            rb.angularDamping = airResistance;

            if (freezeRotation)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }
        else
        {
            if (rb.mass < 1f)
            {
                rb.mass = 1f;
            }
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