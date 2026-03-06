using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    public Rigidbody rb;
    public bool canBeGrabbed = true;
    public int frameNumber = -1;
    public bool isPaper = false;

    [Header("Physics Settings")]
    public float objectMass = 0.1f;
    public float airResistance = 1.5f;
    public bool freezeRotation = true;

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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
    }
}