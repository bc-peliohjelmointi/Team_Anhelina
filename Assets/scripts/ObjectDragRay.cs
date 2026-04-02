using UnityEngine;
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
    public float throwForceMultiplier = 2.5f;
    public int velocitySamples = 5;

    public float puzzlePanelDistance = 3f;
    public GameObject puzzlePanelPrompt;

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

    private PuzzlePanelInteraction currentPanel;
    private AuraHighlight currentAura;

    void Awake()
    {
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, dotColor);
        dotTexture.Apply();
        recentVelocities = new Vector3[velocitySamples];
        if (puzzlePanelPrompt != null) puzzlePanelPrompt.SetActive(false);
    }

    void Update()
    {
        if (!isDragging)
        {
            CheckForPuzzlePanel();
            CheckForAura();
        }

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(interactKey)) && !isDragging)
        {
            if (currentPanel != null)
            {
                currentPanel.EnterPuzzleMode();
                return;
            }

            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
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
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                objectDistance += scroll * scrollSpeed;
                objectDistance = Mathf.Clamp(objectDistance, 1f, maxDistance);
            }
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(interactKey)) && isDragging && currentObject != null)
            ReleaseObject();
    }

    void CheckForPuzzlePanel()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, puzzlePanelDistance))
        {
            PuzzlePanelInteraction panel = hit.collider.GetComponent<PuzzlePanelInteraction>();
            if (panel != null)
            {
                if (currentPanel != panel)
                {
                    currentPanel = panel;
                    if (puzzlePanelPrompt != null) puzzlePanelPrompt.SetActive(true);
                }
                return;
            }
        }
        ClearPanel();
    }

    void CheckForAura()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        AuraHighlight aura = null;

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            aura = hit.collider.GetComponent<AuraHighlight>();

        if (aura != currentAura)
        {
            if (currentAura != null) currentAura.SetGlow(false);
            currentAura = aura;
            if (currentAura != null) currentAura.SetGlow(true);
        }
    }

    void ClearPanel()
    {
        if (currentPanel != null)
        {
            currentPanel = null;
            if (puzzlePanelPrompt != null) puzzlePanelPrompt.SetActive(false);
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

        currentRb.useGravity = false;
        currentRb.linearDamping = 10f;
        currentRb.angularDamping = 10f;
        currentRb.linearVelocity = Vector3.zero;
        currentRb.angularVelocity = Vector3.zero;
        currentRb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (currentObject.freezeRotation)
            currentRb.constraints = RigidbodyConstraints.FreezeRotation;

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
            Vector3 worldGrabPoint = currentObject.transform.TransformPoint(localGrabPoint);
            Vector3 offset = worldGrabPoint - currentRb.position;
            Vector3 targetPosition = targetWorldPoint - offset;
            Vector3 newPosition = currentRb.position + (targetPosition - currentRb.position) * moveForce * Time.fixedDeltaTime;
            currentRb.MovePosition(newPosition);

            Vector3 vel = (targetWorldPoint - lastWorldPoint) / Time.fixedDeltaTime;
            recentVelocities[velocityIndex] = vel;
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
            Vector3 avg = Vector3.zero;
            for (int i = 0; i < velocitySamples; i++) avg += recentVelocities[i];
            avg /= velocitySamples;
            Vector3 throwVel = avg * throwForceMultiplier;
            if (throwVel.magnitude > 25f) throwVel = throwVel.normalized * 25f;
            currentRb.linearVelocity = throwVel;
            currentRb.angularVelocity = Vector3.Cross(throwVel, Vector3.right) * 0.3f;
        }

        currentRb.linearDamping = originalLinearDamping;
        currentRb.angularDamping = originalAngularDamping;
        currentRb.useGravity = true;
        currentRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        currentRb.constraints = RigidbodyConstraints.None;

        currentObject = null;
        currentRb = null;
        isDragging = false;
        localGrabPoint = Vector3.zero;
    }

    Transform GetClosestSlot(Vector3 position)
    {
        Transform best = null;
        float bestDist = slotSnapDistance;
        foreach (Transform slot in slots)
        {
            if (slot == null) continue;
            float d = Vector3.Distance(position, slot.position);
            if (d < bestDist) { bestDist = d; best = slot; }
        }
        return best;
    }

    void OnGUI()
    {
        if (!showCrosshair) return;
        float x = (Screen.width - dotSize) * 0.5f;
        float y = (Screen.height - dotSize) * 0.5f;
        GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), dotTexture);
    }

    void OnDisable()
    {
        ClearPanel();
        if (currentAura != null) { currentAura.SetGlow(false); currentAura = null; }
    }
}