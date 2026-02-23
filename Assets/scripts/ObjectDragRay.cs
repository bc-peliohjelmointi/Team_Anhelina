using UnityEngine;

public class ObjectDragRay : MonoBehaviour
{
    public float maxDistance = 6f;
    public float moveForce = 50f;
    public float scrollSpeed = 2f;
    public float slotSnapDistance = 0.4f;
    public float maxFallSpeed = 10f;
    public Transform[] slots = new Transform[15];
    public int dotSize = 4;
    public Color dotColor = Color.white;
    public bool showCrosshair = true;
    public KeyCode interactKey = KeyCode.E;

    [Header("Throw Settings")]
    public float throwForceMultiplier = 1.5f;
    public float paperThrowMultiplier = 2.5f;
    public int velocitySamples = 5;

    private Texture2D dotTexture;
    private DraggableObject currentObject;
    private Rigidbody currentRb;
    private float objectDistance;
    private bool isDragging = false;
    private Vector3 grabOffset;

    private Vector3[] recentVelocities;
    private int velocityIndex = 0;
    private Vector3 lastGrabPoint;
    private bool isPaper = false;
    private float originalLinearDamping;
    private float originalAngularDamping;

    void Awake()
    {
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, dotColor);
        dotTexture.Apply();

        recentVelocities = new Vector3[velocitySamples];
    }

    void Update()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactKey)) && !isDragging)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                TVButton tvButton = hit.collider.GetComponent<TVButton>();
                if (tvButton != null)
                {
                    tvButton.Press();
                    return;
                }

                PSButton psButton = hit.collider.GetComponent<PSButton>();
                if (psButton != null)
                {
                    psButton.Press();
                    return;
                }

                DraggableObject draggable = hit.collider.GetComponent<DraggableObject>();
                if (draggable != null && draggable.canBeGrabbed)
                {
                    currentObject = draggable;
                    currentRb = draggable.rb;
                    isPaper = draggable.isPaper;

                    if (isPaper)
                    {
                        originalLinearDamping = draggable.airResistance;
                        originalAngularDamping = draggable.airResistance;
                    }

                    StartGrab(hit);
                }
            }
        }

        if (isDragging && currentObject != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                objectDistance += scroll * scrollSpeed;
                objectDistance = Mathf.Clamp(objectDistance, 1f, maxDistance);
            }
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(interactKey)) && isDragging && currentObject != null)
        {
            ReleaseObject();
        }
    }

    void StartGrab(RaycastHit hit)
    {
        objectDistance = Vector3.Distance(transform.position, hit.point);
        grabOffset = currentRb.position - hit.point;
        lastGrabPoint = hit.point;

        currentRb.useGravity = false;
        currentRb.linearDamping = 10f;
        currentRb.angularDamping = 5f;
        currentRb.linearVelocity = Vector3.zero;
        currentRb.angularVelocity = Vector3.zero;
        currentRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (!isPaper)
        {
            currentObject.canBePushed = false;
        }

        for (int i = 0; i < velocitySamples; i++)
        {
            recentVelocities[i] = Vector3.zero;
        }
        velocityIndex = 0;

        isDragging = true;
    }

    void FixedUpdate()
    {
        if ((Input.GetMouseButton(0) || Input.GetKey(interactKey)) && isDragging && currentObject != null && currentRb != null)
        {
            Vector3 currentGrabPoint = transform.position + transform.forward * objectDistance;
            Vector3 targetPosition = currentGrabPoint - grabOffset;

            Vector3 direction = targetPosition - currentRb.position;
            Vector3 newPosition = currentRb.position + direction * (moveForce * Time.fixedDeltaTime * 0.1f);

            Vector3 moveDirection = newPosition - currentRb.position;
            float moveDistance = moveDirection.magnitude;

            if (moveDistance > 0.001f)
            {
                if (Physics.Raycast(currentRb.position, moveDirection.normalized,
                    out RaycastHit hit, moveDistance + 0.1f))
                {
                    if (hit.collider.gameObject != currentObject.gameObject)
                    {
                        newPosition = hit.point - moveDirection.normalized * 0.1f;
                    }
                }

                currentRb.MovePosition(newPosition);
            }

            Vector3 grabPointVelocity = (currentGrabPoint - lastGrabPoint) / Time.fixedDeltaTime;
            recentVelocities[velocityIndex] = grabPointVelocity;
            velocityIndex = (velocityIndex + 1) % velocitySamples;
            lastGrabPoint = currentGrabPoint;
        }

        if (!isDragging && currentRb != null && !isPaper)
        {
            LimitFallSpeed(currentRb);
        }
    }

    void ReleaseObject()
    {
        if (currentObject == null || currentRb == null) return;

        Transform slot = GetClosestSlot(currentRb.position);
        if (slot != null)
        {
            currentRb.position = slot.position;
            currentRb.linearVelocity = Vector3.zero;
            currentRb.angularVelocity = Vector3.zero;
        }
        else
        {
            Vector3 averageVelocity = Vector3.zero;
            for (int i = 0; i < velocitySamples; i++)
            {
                averageVelocity += recentVelocities[i];
            }
            averageVelocity /= velocitySamples;

            float multiplier = isPaper ? paperThrowMultiplier : throwForceMultiplier;
            Vector3 throwVelocity = averageVelocity * multiplier;

            float maxSpeed = isPaper ? 25f : 20f;
            if (throwVelocity.magnitude > maxSpeed)
            {
                throwVelocity = throwVelocity.normalized * maxSpeed;
            }

            currentRb.linearVelocity = throwVelocity;

            if (!isPaper)
            {
                Vector3 torque = Vector3.Cross(throwVelocity, Vector3.up) * 0.1f;
                currentRb.angularVelocity = torque;
            }
            else
            {
                Vector3 torque = Vector3.Cross(throwVelocity, Vector3.right) * 0.3f;
                currentRb.angularVelocity = torque;
            }
        }

        if (isPaper)
        {
            currentRb.linearDamping = originalLinearDamping;
            currentRb.angularDamping = originalAngularDamping;
        }
        else
        {
            currentRb.linearDamping = 0.5f;
            currentRb.angularDamping = 0.5f;
            currentObject.canBePushed = true;
        }

        currentRb.useGravity = true;
        currentRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (!isPaper)
        {
            StartCoroutine(MonitorFallingObject(currentObject));
        }

        currentObject = null;
        currentRb = null;
        isDragging = false;
        grabOffset = Vector3.zero;
        isPaper = false;
    }

    System.Collections.IEnumerator MonitorFallingObject(DraggableObject obj)
    {
        float timer = 0f;
        float maxMonitorTime = 5f;

        while (timer < maxMonitorTime && obj != null && obj.rb != null)
        {
            LimitFallSpeed(obj.rb);

            if (obj.rb.linearVelocity.magnitude < 0.1f)
            {
                obj.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                yield break;
            }

            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (obj != null && obj.rb != null)
        {
            obj.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    void LimitFallSpeed(Rigidbody rb)
    {
        if (rb.linearVelocity.magnitude > maxFallSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxFallSpeed;
        }
    }

    Transform GetClosestSlot(Vector3 position)
    {
        Transform bestSlot = null;
        float bestDistance = slotSnapDistance;

        foreach (Transform slot in slots)
        {
            if (slot == null) continue;

            float d = Vector3.Distance(position, slot.position);
            if (d < bestDistance)
            {
                bestDistance = d;
                bestSlot = slot;
            }
        }
        return bestSlot;
    }

    void OnGUI()
    {
        if (showCrosshair)
        {
            float x = (Screen.width - dotSize) * 0.5f;
            float y = (Screen.height - dotSize) * 0.5f;
            GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), dotTexture);
        }
    }
}