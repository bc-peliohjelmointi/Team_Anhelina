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
    private Texture2D dotTexture;
    private DraggableObject currentObject;
    private float objectDistance;
    private bool isDragging = false;

    void Awake()
    {
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, dotColor);
        dotTexture.Apply();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isDragging)
        {
            Ray ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                DraggableObject draggable = hit.collider.GetComponent<DraggableObject>();
                if (draggable != null)
                {
                    currentObject = draggable;
                    objectDistance = Vector3.Distance(transform.position, hit.point);

                    currentObject.rb.useGravity = false;
                    currentObject.rb.linearDamping = 10f;
                    currentObject.rb.angularDamping = 5f; 
                    currentObject.rb.linearVelocity = Vector3.zero;
                    currentObject.rb.angularVelocity = Vector3.zero;

                    currentObject.rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                    isDragging = true;
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

        if (Input.GetMouseButtonUp(0) && isDragging && currentObject != null)
        {
            ReleaseObject();
        }
    }

    void FixedUpdate()
    {
        if (Input.GetMouseButton(0) && isDragging && currentObject != null)
        {
            Vector3 targetPosition = transform.position + transform.forward * objectDistance;
            Vector3 direction = targetPosition - currentObject.rb.position;

            Vector3 newPosition = currentObject.rb.position + direction * (moveForce * Time.fixedDeltaTime * 0.1f);

            Vector3 moveDirection = newPosition - currentObject.rb.position;
            float moveDistance = moveDirection.magnitude;

            if (moveDistance > 0.001f)
            {
                if (Physics.Raycast(currentObject.rb.position, moveDirection.normalized,
                    out RaycastHit hit, moveDistance + 0.1f))
                {
                    if (hit.collider.gameObject != currentObject.gameObject)
                    {
                        newPosition = hit.point - moveDirection.normalized * 0.1f;
                    }
                }

                currentObject.rb.MovePosition(newPosition);
            }

        }
    }

    void ReleaseObject()
    {
        if (currentObject == null) return;

        Transform slot = GetClosestSlot(currentObject.transform.position);
        if (slot != null)
        {
            currentObject.rb.position = slot.position;
            currentObject.rb.linearVelocity = Vector3.zero;
            currentObject.rb.angularVelocity = Vector3.zero;
        }

        currentObject.rb.linearDamping = 0f;
        currentObject.rb.angularDamping = 0.05f;
        currentObject.rb.useGravity = true;

        currentObject.rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        currentObject = null;
        isDragging = false;
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
        float x = (Screen.width - dotSize) * 0.5f;
        float y = (Screen.height - dotSize) * 0.5f;
        GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), dotTexture);
    }
}