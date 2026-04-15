using UnityEngine;

// simple component to mark objects as draggable
// put this on any paper/object you want the player to be able to pick up
public class DraggableObject : MonoBehaviour
{
    public Rigidbody rb;
    public bool canBeGrabbed = true;
    public int frameNumber = -1; // which frame this paper is, -1 means not assigned
    public bool isPaper = false;  // only paper objects can be grabbed by drag system

    [Header("Physics Settings")]
    public float objectMass = 0.1f;       // light so it feels like paper
    public float airResistance = 1.5f;    // a bit of drag so it doesnt slide forever
    public bool freezeRotation = true;    // usually want paper to stay flat

    void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>(); // grab it if not set manually
        }

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.mass = objectMass;
            rb.linearDamping = airResistance;
            rb.angularDamping = airResistance;

            if (freezeRotation)
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation; // stays upright
            }
            else
            {
                rb.constraints = RigidbodyConstraints.None;
            }
        }
    }
}