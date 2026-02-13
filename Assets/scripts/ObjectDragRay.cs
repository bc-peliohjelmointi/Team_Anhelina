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
                    currentObject.canBePushed = false;

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

        if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(interactKey)) && isDragging && currentObject != null)
        {
            ReleaseObject();
        }
    }

    void FixedUpdate()
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
                    currentObject.canBePushed = false;

                    isDragging = true;
                }
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
        else
        {
            currentObject.rb.linearVelocity = Vector3.ClampMagnitude(currentObject.rb.linearVelocity, 2f);
        }

        currentObject.rb.linearDamping = 0.5f;
        currentObject.rb.angularDamping = 0.5f;
        currentObject.rb.useGravity = true;
        currentObject.rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        currentObject.canBePushed = true;

        StartCoroutine(MonitorFallingObject(currentObject));

        currentObject = null;
        isDragging = false;
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