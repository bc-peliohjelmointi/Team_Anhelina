using UnityEngine;
using System.Collections;
public class BoardController2 : MonoBehaviour
{
    public Transform playerCamera;
    public GameObject boardObject;
    public Vector3 visiblePosition = new Vector3(0f, -0.4f, 0.6f);
    public Vector3 visibleRotation = new Vector3(30f, 0f, 0f);
    public Vector3 hiddenPosition = new Vector3(0f, -1.5f, 0.6f);
    public Vector3 hiddenRotation = new Vector3(30f, 0f, 0f);
    public float startShowAngle = 65f;
    public float fullyVisibleAngle = 80f;
    public float smoothSpeed = 10f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public KeyCode tabKey = KeyCode.Tab;
    public float snapDuration = 0.3f;
    private GameObject cardViewObject;
    private bool hasCard = false, tabActive = false, isSnapping = false;
    private int currentTabItem = 0;

    void Start()
    {
        if (boardObject != null && playerCamera != null)
        {
            boardObject.transform.SetParent(playerCamera);
            boardObject.transform.localPosition = hiddenPosition;
            boardObject.transform.localRotation = Quaternion.Euler(hiddenRotation);
            Rigidbody rb = boardObject.GetComponent<Rigidbody>(); if (rb != null) Destroy(rb);
            Collider col = boardObject.GetComponent<Collider>(); if (col != null) col.enabled = false;
        }
    }

    void Update()
    {
        if (playerCamera == null || boardObject == null) return;
        if (Input.GetKeyDown(tabKey) && !isSnapping) HandleTab();
        UpdateTransform();
    }

    void HandleTab()
    {
        float angle = GetAngle();
        if (!tabActive) { tabActive = true; StartCoroutine(SnapCamera(fullyVisibleAngle)); }
        else if (angle >= fullyVisibleAngle - 5f)
        {
            if (hasCard) { currentTabItem = (currentTabItem + 1) % 2; boardObject.SetActive(currentTabItem == 0); if (cardViewObject != null) cardViewObject.SetActive(currentTabItem == 1); }
            else { tabActive = false; StartCoroutine(SnapCamera(0f)); }
        }
    }

    public void OnCardPickedUp(GameObject cardView)
    {
        hasCard = true; cardViewObject = cardView;
        if (cardViewObject != null) cardViewObject.SetActive(false);
    }

    public bool HasCard() => hasCard;

    void UpdateTransform()
    {
        float cv = slideCurve.Evaluate(CalculateSlide(GetAngle()));
        boardObject.transform.localPosition = Vector3.Lerp(boardObject.transform.localPosition, Vector3.Lerp(hiddenPosition, visiblePosition, cv), Time.deltaTime * smoothSpeed);
        boardObject.transform.localRotation = Quaternion.Slerp(boardObject.transform.localRotation, Quaternion.Lerp(Quaternion.Euler(hiddenRotation), Quaternion.Euler(visibleRotation), cv), Time.deltaTime * smoothSpeed);
    }

    float GetAngle() { float r = playerCamera.localEulerAngles.x; if (r > 180f) r -= 360f; return Mathf.Clamp(r, -90f, 90f); }
    float CalculateSlide(float a) { if (a < startShowAngle) return 0f; if (a >= fullyVisibleAngle) return 1f; return (a - startShowAngle) / (fullyVisibleAngle - startShowAngle); }

    IEnumerator SnapCamera(float target)
    {
        isSnapping = true; float start = GetAngle(), elapsed = 0f;
        while (elapsed < snapDuration) { elapsed += Time.deltaTime; playerCamera.localRotation = Quaternion.Euler(Mathf.Lerp(start, target, elapsed / snapDuration), 0f, 0f); yield return null; }
        playerCamera.localRotation = Quaternion.Euler(target, 0f, 0f); isSnapping = false;
    }
}