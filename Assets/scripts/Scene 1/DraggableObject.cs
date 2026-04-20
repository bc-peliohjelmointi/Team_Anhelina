using UnityEngine;
using System.Collections;

// simple component to mark objects as draggable
// put this on any paper/object you want the player to be able to pick up
public class DraggableObject : MonoBehaviour
{
    public Rigidbody rb;
    public bool canBeGrabbed = true;
    public int frameNumber = -1; // which frame this paper is, -1 means not assigned
    public bool isPaper = false; // only paper objects can be grabbed by drag system

    [Header("Physics Settings")]
    public float objectMass = 0.1f; // light so it feels like paper
    public float airResistance = 1.5f; // lowered a bit so paper has nice floaty continuation after release (not instant stop)
    public bool freezeRotationDuringDrag = true; // freeze rotation only while holding it

    [Header("Boundary Settings")]
    // assign this to a transform anywhere in scene - thats where paper returns to
    public Transform returnPoint;
    // how many seconds after leaving bounds before paper teleports back
    public float returnDelay = 3f;
    // the boundary trigger collider - create an empty GameObject with a large
    // paper teleports back when it leaves this collider
    public Collider boundaryCollider;

    [Header("Drag System - fixes going through walls + realistic paper throw")]
    // new spring-based dragging so the object never passes through colliders
    public float dragSpringStrength = 68f;   // increased for sharper, less "viscous" feel when dragging
    public float dragDamping = 5f;           // lowered so it feels more responsive and less sticky

    // internal stuff
    private Vector3 returnPosition;
    private Quaternion returnRotation;
    private bool isOutOfBounds = false;
    private Coroutine returnCoroutine;

    private bool isBeingDragged = false;     // flag so we know the player is holding it
    private Vector3 dragTargetPosition;      // target position updated every frame from ObjectDragRay

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>(); // grab it if not set manually

        if (rb != null)
        {
            // continuous dynamic prevents tunneling through colliders while moving fast
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.mass = objectMass;
            rb.linearDamping = airResistance;
            rb.angularDamping = airResistance + 1f; // paper stops spinning quickly when it lands
            rb.constraints = RigidbodyConstraints.None; // rotation is free by default for realistic throws
        }

        // save initial position as return point if none assigned
        SaveReturnPoint();
    }

    void SaveReturnPoint()
    {
        if (returnPoint != null)
        {
            // use the assigned return transform
            returnPosition = returnPoint.position;
            returnRotation = returnPoint.rotation;
        }
        else
        {
            // fall back to spawn position
            returnPosition = transform.position;
            returnRotation = transform.rotation;
        }
    }

    public void StartDragging()
    {
        if (rb == null) return;

        isBeingDragged = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // keep the paper flat in the hand (feels natural)
        if (freezeRotationDuringDrag)
            rb.constraints = RigidbodyConstraints.FreezeRotation;

        dragTargetPosition = transform.position;
    }

    public void UpdateDragTarget(Vector3 targetPosition)
    {
        if (!isBeingDragged) return;
        dragTargetPosition = targetPosition;
    }

    public void StopDragging()
    {
        if (rb == null || !isBeingDragged) return;
        isBeingDragged = false;
        OnReleased(); // everything that happens when you let go is now here
    }

    void FixedUpdate()
    {
        if (isBeingDragged && rb != null)
        {
            // spring force pulls the object toward the mouse position
            // Unity physics still checks all colliders properly
            Vector3 displacement = dragTargetPosition - rb.position;
            Vector3 springForce = displacement * dragSpringStrength;
            Vector3 dampingForce = -rb.linearVelocity * dragDamping;

            rb.AddForce(springForce + dampingForce, ForceMode.Acceleration);
        }
    }

    // called when player releases the object
    public void OnReleased()
    {
        if (rb == null) return;

        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (isPaper)
        {
            rb.constraints = RigidbodyConstraints.None; // unlock rotation so it can tumble and flutter

            Vector3 vel = rb.linearVelocity;
            if (vel.magnitude > 0.8f)
            {
                // adds that nice flutter/tumble effect when you throw it
                Vector3 flutterTorque = Vector3.Cross(vel.normalized, transform.up) * (vel.magnitude * 4.5f);
                flutterTorque += new Vector3(
                    Random.Range(-vel.magnitude * 2.5f, vel.magnitude * 2.5f),
                    Random.Range(-vel.magnitude * 1.2f, vel.magnitude * 1.2f),
                    Random.Range(-vel.magnitude * 2.5f, vel.magnitude * 2.5f)
                );

                rb.AddTorque(flutterTorque, ForceMode.Impulse);
            }
        }
        else if (freezeRotationDuringDrag)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }

    // called when paper exits the boundary trigger collider
    void OnTriggerExit(Collider other)
    {
        // only react to the assigned boundary collider
        if (boundaryCollider != null && other != boundaryCollider) return;
        // if player is still holding it, don't start the return timer
        if (isBeingDragged) return;

        if (!isOutOfBounds)
        {
            isOutOfBounds = true;
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnAfterDelay());
        }
    }

    // called when paper comes back inside boundary
    void OnTriggerEnter(Collider other)
    {
        if (boundaryCollider != null && other != boundaryCollider) return;
        if (isOutOfBounds)
        {
            isOutOfBounds = false;
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
        }
    }

    IEnumerator ReturnAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);
        if (isOutOfBounds)
            TeleportToReturn();
    }

    void TeleportToReturn()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        transform.position = returnPosition;
        transform.rotation = returnRotation;
        isOutOfBounds = false;
        returnCoroutine = null;
    }

    // call this if you move the return point at runtime
    public void UpdateReturnPoint()
    {
        SaveReturnPoint();
    }
}