using UnityEngine;

public class teapothold : MonoBehaviour
{
    public Rigidbody rb;
    public float grabDistance = 3f;
    public float holdDistance = 2f;
    public float holdSmoothing = 10f;

    private bool isHeld = false;
    private Transform playerCamera;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        playerCamera = Camera.main.transform;
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            TryGrab();
        }


        if (Input.GetMouseButtonUp(1))
        {
            if (isHeld) Drop();
        }


        if (isHeld)
        {
            Vector3 targetPos = playerCamera.position + playerCamera.forward * holdDistance;
            rb.linearVelocity = (targetPos - transform.position) * holdSmoothing;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void TryGrab()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.transform == this.transform)
            {
                isHeld = true;
                rb.useGravity = false;
                rb.linearDamping = 10f;
            }
        }
    }

    void Drop()
    {
        isHeld = false;
        rb.useGravity = true;
        rb.linearDamping = 0.5f;
    }
}