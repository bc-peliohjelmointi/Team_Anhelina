using UnityEngine;
// handles grabbing, dragging, snapping paper objects
// detects buttons and highlights hovered objects with aura
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
    public float auraCheckDistance = 6f;

    [Header("Controls while holding paper")]
    // Z/X = rotate left/right, Q/E = push/pull distance
    public float rotationSpeed = 180f;
    public float distanceAdjustSpeed = 4f;

    [Header("Zoom Settings")]
    // how much to zoom in when right mouse is held
    public float zoomFOV = 30f;
    // how fast the zoom transitions in and out
    public float zoomSpeed = 10f;

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

    // zoom state
    private Camera cam;
    private float defaultFOV;
    private bool isZooming = false;

    void Awake()
    {
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, dotColor);
        dotTexture.Apply();
        recentVelocities = new Vector3[velocitySamples];

        // grab the camera and save its default FOV
        cam = GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;
        if (cam != null) defaultFOV = cam.fieldOfView;
    }

    void Update()
    {
        if (!isDragging)
            CheckForAura();

        HandleZoom();

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactKey)) && !isDragging)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                // check light switch first
                LightSwitch lightSwitch = hit.collider.GetComponent<LightSwitch>();
                if (lightSwitch != null) { lightSwitch.Toggle(); return; }

                TVButton tvButton = hit.collider.GetComponent<TVButton>();
                if (tvButton != null) { tvButton.Press(); return; }

                PSButton psButton = hit.collider.GetComponent<PSButton>();
                if (psButton != null) { psButton.Press(); return; }

                DraggableObject draggable = hit.collider.GetComponent<DraggableObject>();
                if (draggable != null && draggable.canBeGrabbed && draggable.isPaper)
                {
                    currentObject = draggable;
                    currentRb = draggable.rb;
                    StartGrab(hit);
                }
            }
        }

        if (isDragging && currentObject != null)
        {
            // scroll to push or pull
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                objectDistance += scroll * scrollSpeed;
                objectDistance = Mathf.Clamp(objectDistance, 1f, maxDistance);
            }

            // rotate with Z/X
            float rotDelta = 0f;
            if (Input.GetKey(KeyCode.Z)) rotDelta -= rotationSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.X)) rotDelta += rotationSpeed * Time.deltaTime;
            if (rotDelta != 0f)
                currentObject.transform.Rotate(0f, rotDelta, 0f, Space.Self);

            // adjust distance with Q/E
            float distanceDelta = 0f;
            if (Input.GetKey(KeyCode.Q)) distanceDelta += distanceAdjustSpeed * Time.deltaTime;
            if (Input.GetKey(KeyCode.E)) distanceDelta -= distanceAdjustSpeed * Time.deltaTime;
            objectDistance = Mathf.Clamp(objectDistance + distanceDelta, 1f, maxDistance);
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(interactKey)) && isDragging && currentObject != null)
            ReleaseObject();
    }

    // smoothly zooms in when right mouse is held, zooms back out on release
    void HandleZoom()
    {
        if (cam == null) return;

        isZooming = Input.GetMouseButton(1);
        float targetFOV = isZooming ? zoomFOV : defaultFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    // shoots a ray forward and glows whatever the player is looking at
    void CheckForAura()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        AuraHighlight aura = null;

        if (Physics.Raycast(ray, out RaycastHit hit, auraCheckDistance))
        {
            aura = hit.collider.GetComponent<AuraHighlight>();
            if (aura == null) aura = hit.collider.GetComponentInParent<AuraHighlight>();
            if (aura == null) aura = hit.collider.GetComponentInChildren<AuraHighlight>();
        }

        // only swap if something actually changed
        if (aura != currentAura)
        {
            if (currentAura != null) currentAura.SetGlow(false);
            currentAura = aura;
            if (currentAura != null) currentAura.SetGlow(true);
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

        currentObject.StartDragging();

        currentRb.linearVelocity = Vector3.zero;
        currentRb.angularVelocity = Vector3.zero;

        for (int i = 0; i < velocitySamples; i++)
            recentVelocities[i] = Vector3.zero;

        velocityIndex = 0;
        isDragging = true;
    }

    void FixedUpdate()
    {
        if ((Input.GetMouseButton(0) || Input.GetKey(interactKey)) && isDragging && currentObject != null && currentRb != null)
        {
            Vector3 targetWorldPoint = transform.position + transform.forward * objectDistance;

            // offset so the exact grab point follows the ray, not the pivot
            Vector3 worldGrabPoint = currentObject.transform.TransformPoint(localGrabPoint);
            Vector3 offset = worldGrabPoint - currentRb.position;
            Vector3 targetPosition = targetWorldPoint - offset;

            currentObject.UpdateDragTarget(targetPosition);

            // sample velocity for throw on release
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
            // snap into slot and freeze it there
            currentRb.position = slot.position;
            currentRb.rotation = originalRotation;
            currentRb.linearVelocity = Vector3.zero;
            currentRb.angularVelocity = Vector3.zero;
        }
        else
        {
            // average last few frames of movement for a natural throw
            Vector3 averageVelocity = Vector3.zero;
            for (int i = 0; i < velocitySamples; i++)
                averageVelocity += recentVelocities[i];
            averageVelocity /= velocitySamples;

            Vector3 throwVelocity = averageVelocity * throwForceMultiplier;
            if (throwVelocity.magnitude > 25f)
                throwVelocity = throwVelocity.normalized * 25f;

            currentRb.linearVelocity = throwVelocity;
            currentRb.angularVelocity = Vector3.Cross(throwVelocity, Vector3.right) * 0.3f;
        }

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

    void OnDisable()
    {
        // restore FOV if script gets disabled mid-zoom
        if (cam != null)
            cam.fieldOfView = defaultFOV;

        if (currentAura != null)
        {
            currentAura.SetGlow(false);
            currentAura = null;
        }
    }
}