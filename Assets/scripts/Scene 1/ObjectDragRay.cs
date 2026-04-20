using UnityEngine;
// handles grabbing, dragging, snapping paper objects
// also detects buttons, puzzle panels, and now highlights hovered objects with aura
public class ObjectDragRay : MonoBehaviour
{
    public float maxDistance = 6f;
    public float moveForce = 50f;
    public float scrollSpeed = 2f;
    public float slotSnapDistance = 0.4f;
    public Transform[] slots = new Transform[15];
    public int dotSize = 4;
    public Color dotColor = Color.white;
    public bool showCrosshair = true;
    public KeyCode interactKey = KeyCode.E;
    public LayerMask paperLayer;

    [Header("Throw Settings")]
    public float throwForceMultiplier = 2.5f;
    public int velocitySamples = 5;

    [Header("Aura / Highlight")]
    public float auraCheckDistance = 6f; // can be different from max grab distance

    [Header("Controls while holding paper")]
    // Z = rotate paper left (counter-clockwise on Y axis)
    // X = rotate paper right (clockwise on Y axis)
    // Q = move paper AWAY from camera (increase distance)
    // E = move paper TOWARD camera (decrease distance)
    // full 360 degree rotation is possible - just hold the key
    public float rotationSpeed = 180f;      // degrees per second
    public float distanceAdjustSpeed = 4f;  // how fast Q/E changes the hold distance

    private Texture2D dotTexture;
    private DraggableObject currentObject;
    private Rigidbody currentRb;
    private float objectDistance;
    private bool isDragging = false;
    private Vector3 localGrabPoint;
    private Quaternion originalRotation;
    private Vector3[] recentVelocities;
    private int velocityIndex = 0;
    private Vector3 lastWorldPoint;
    private float originalLinearDamping;
    private float originalAngularDamping;
    private AuraHighlight currentAura;

    void Awake()
    {
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, dotColor);
        dotTexture.Apply();
        recentVelocities = new Vector3[velocitySamples];
    }

    void Update()
    {
        // only check for aura when not dragging something
        if (!isDragging)
        {
            CheckForAura();
        }

        // grab input
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactKey)) && !isDragging)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                // check buttons first
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

                // then try to grab paper
                DraggableObject draggable = hit.collider.GetComponent<DraggableObject>();
                if (draggable != null && draggable.canBeGrabbed && draggable.isPaper)
                {
                    currentObject = draggable;
                    currentRb = draggable.rb;
                    StartGrab(hit);
                }
            }
        }

        // scroll to push/pull object 
        if (isDragging && currentObject != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                objectDistance += scroll * scrollSpeed;
                objectDistance = Mathf.Clamp(objectDistance, 1f, maxDistance);
            }

            // rotate paper left/right with Z/X
            float rotDelta = 0f;
            if (Input.GetKey(KeyCode.Z)) rotDelta -= rotationSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.X)) rotDelta += rotationSpeed * Time.deltaTime;
            if (rotDelta != 0f && currentObject != null)
            {
                currentObject.transform.Rotate(0f, rotDelta, 0f, Space.Self);
            }

            // NEW: Q/E now moves paper toward/away from camera 
            float distanceDelta = 0f;
            if (Input.GetKey(KeyCode.Q)) distanceDelta += distanceAdjustSpeed * Time.deltaTime; // Q = farther
            if (Input.GetKey(KeyCode.E)) distanceDelta -= distanceAdjustSpeed * Time.deltaTime; // E = closer
            objectDistance += distanceDelta;
            objectDistance = Mathf.Clamp(objectDistance, 1f, maxDistance);
        }

        // release input
        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(interactKey)) && isDragging && currentObject != null)
        {
            ReleaseObject();
        }
    }

    // checks what the player is looking at and updates aura glow accordingly
    void CheckForAura()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        AuraHighlight aura = null;
        if (Physics.Raycast(ray, out RaycastHit hit, auraCheckDistance))
        {
            // try to get aura from hit object or its parent
            aura = hit.collider.GetComponent<AuraHighlight>();
            if (aura == null)
            {
                aura = hit.collider.GetComponentInParent<AuraHighlight>();
            }
        }
        // only update if something changed - dont call SetGlow every frame for nothing
        if (aura != currentAura)
        {
            if (currentAura != null)
            {
                currentAura.SetGlow(false); // turn off old one
            }
            currentAura = aura;
            if (currentAura != null)
            {
                currentAura.SetGlow(true); // turn on new one
            }
        }
    }

    void StartGrab(RaycastHit hit)
    {
        objectDistance = Vector3.Distance(transform.position, hit.point);
        localGrabPoint = currentObject.transform.InverseTransformPoint(hit.point);
        originalRotation = currentObject.transform.rotation;
        lastWorldPoint = hit.point;
        originalLinearDamping = currentRb.linearDamping;
        originalAngularDamping = currentRb.angularDamping;

        // switch to the new physical drag system
        currentObject.StartDragging();

        currentRb.linearVelocity = Vector3.zero;
        currentRb.angularVelocity = Vector3.zero;

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
            // target point is now purely along the camera ray 
            Vector3 targetWorldPoint = transform.position + transform.forward * objectDistance;

            Vector3 worldGrabPoint = currentObject.transform.TransformPoint(localGrabPoint);
            Vector3 offset = worldGrabPoint - currentRb.position;
            Vector3 targetPosition = targetWorldPoint - offset;

            // send the target to DraggableObject 
            currentObject.UpdateDragTarget(targetPosition);

            // keep velocity sampling for throw calculation 
            Vector3 velocity = (targetWorldPoint - lastWorldPoint) / Time.fixedDeltaTime;
            recentVelocities[velocityIndex] = velocity;
            velocityIndex = (velocityIndex + 1) % velocitySamples;
            lastWorldPoint = targetWorldPoint;
        }
    }

    void ReleaseObject()
    {
        if (currentObject == null || currentRb == null) return;

        Transform slot = GetClosestSlot(currentRb.position);
        if (slot != null)
        {
            currentRb.position = slot.position;
            currentRb.rotation = originalRotation;
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

            Vector3 throwVelocity = averageVelocity * throwForceMultiplier;
            float maxSpeed = 25f;
            if (throwVelocity.magnitude > maxSpeed)
            {
                throwVelocity = throwVelocity.normalized * maxSpeed;
            }
            currentRb.linearVelocity = throwVelocity;
            currentRb.angularVelocity = Vector3.Cross(throwVelocity, Vector3.right) * 0.3f;
        }

        // release through the new system 
        currentObject.StopDragging();

        currentObject = null;
        currentRb = null;
        isDragging = false;
        localGrabPoint = Vector3.zero;
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

    // make sure we turn off any active glow when this script is disabled
    void OnDisable()
    {
        if (currentAura != null)
        {
            currentAura.SetGlow(false);
            currentAura = null;
        }
    }
}