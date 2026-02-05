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
                    currentObject.rb.linearVelocity = Vector3.zero;
                    isDragging = true;
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging && currentObject != null)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            objectDistance += scroll * scrollSpeed;
            objectDistance = Mathf.Clamp(objectDistance, 1f, maxDistance);

            Vector3 targetPosition = transform.position + transform.forward * objectDistance;
            Vector3 direction = targetPosition - currentObject.rb.position;
            currentObject.rb.AddForce(direction * moveForce, ForceMode.Force);
        }


        if (Input.GetMouseButtonUp(0) && isDragging && currentObject != null)
        {
            ReleaseObject();
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
        }

        currentObject.rb.linearDamping = 0f;
        currentObject.rb.useGravity = true;
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