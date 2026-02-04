using UnityEngine;

public class ObjectDragRayWithDot : MonoBehaviour
{
    public float maxDistance = 5f;
    public float moveSpeed = 15f;

    public int dotSize = 4;
    public Color dotColor = Color.white;

    private Texture2D dotTexture;
    private DraggableObject currentObject;
    private float objectDistance;

    void Awake()
    {
        dotTexture = new Texture2D(1, 1);
        dotTexture.SetPixel(0, 0, dotColor);
        dotTexture.Apply();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
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
                    currentObject.rb.linearVelocity = Vector3.zero;
                }
            }
        }

        if (Input.GetMouseButton(0) && currentObject != null)
        {
            Vector3 targetPosition = transform.position + transform.forward * objectDistance;
            Vector3 newPosition = Vector3.Lerp(
                currentObject.rb.position,
                targetPosition,
                Time.deltaTime * moveSpeed
            );
            currentObject.rb.MovePosition(newPosition);
        }

        if (Input.GetMouseButtonUp(0) && currentObject != null)
        {
            currentObject.rb.useGravity = true;
            currentObject = null;
        }
    }

    void OnGUI()
    {
        float x = (Screen.width - dotSize) * 0.5f;
        float y = (Screen.height - dotSize) * 0.5f;
        GUI.DrawTexture(new Rect(x, y, dotSize, dotSize), dotTexture);
    }
}
