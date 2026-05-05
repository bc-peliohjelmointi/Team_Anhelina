using UnityEngine;
using UnityEngine.UI;
// handles the key card view when player switches to it with Tab
// card can be rotated by holding left mouse button and dragging
// shows card front with access level, name, barcode decoration
// attach this to the cardViewModel GameObject
// BoardController calls OnCardPickedUp which enables this view
public class KeyCardViewHandler : MonoBehaviour
{
    // ---- rotation settings ----
    // how fast the card rotates when dragging mouse
    public float rotateSpeed = 3f;
    // how fast card returns to default rotation when mouse released
    public float returnSpeed = 4f;
    // default rotation to return to when not dragging
    public Vector3 defaultRotation = new Vector3(0f, 0f, 0f);
    // max rotation limits so card doesnt flip fully upside down
    public float maxVerticalAngle = 60f;
    public float maxHorizontalAngle = 80f;

    // ---- card UI elements (optional cosmetic) ----
    // name printed on the card
    public Text cardNameText;
    // access level text
    public Text accessLevelText;
    // what name to show on the card
    public string cardHolderName = "СОТРУДНИК";
    public string accessLevel = "УРОВЕНЬ 3";

    // ---- hint ----
    // small "ЛКМ - вращать" hint shown while viewing card
    public GameObject rotateHint;

    private bool isDragging = false;
    private float currentYaw = 0f;
    private float currentPitch = 0f;
    private Vector3 lastMousePos;

    void Start()
    {
        // fill in card text
        if (cardNameText != null) cardNameText.text = cardHolderName;
        if (accessLevelText != null) accessLevelText.text = accessLevel;
        if (rotateHint != null) rotateHint.SetActive(true);
    }

    void OnEnable()
    {
        // reset rotation each time card is shown
        currentYaw = 0f;
        currentPitch = 0f;
        transform.localRotation = Quaternion.Euler(defaultRotation);
    }

    void Update()
    {
        // left mouse button drag to rotate card
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0)) isDragging = false;
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;
            currentYaw += delta.x * rotateSpeed * Time.deltaTime * 60f;
            currentPitch -= delta.y * rotateSpeed * Time.deltaTime * 60f;
            // clamp so card doesnt rotate too far
            currentYaw = Mathf.Clamp(currentYaw, -maxHorizontalAngle, maxHorizontalAngle);
            currentPitch = Mathf.Clamp(currentPitch, -maxVerticalAngle, maxVerticalAngle);
        }
        else
        {
            // smoothly return to default rotation when not dragging
            currentYaw = Mathf.Lerp(currentYaw, 0f, Time.deltaTime * returnSpeed);
            currentPitch = Mathf.Lerp(currentPitch, 0f, Time.deltaTime * returnSpeed);
        }
        // apply rotation relative to default
        transform.localRotation = Quaternion.Euler(
            defaultRotation.x + currentPitch,
            defaultRotation.y + currentYaw,
            defaultRotation.z);
    }
}