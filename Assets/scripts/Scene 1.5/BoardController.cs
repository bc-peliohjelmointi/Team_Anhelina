using UnityEngine;
using System.Collections;
// controls the clipboard that appears when player looks down past 65 degrees
// Tab snaps camera down instantly to view it
// pressing Tab again hides it
// after card is picked up Tab switches between clipboard and card view
public class BoardController : MonoBehaviour
{
    // the players camera transform
    public Transform playerCamera;
    // the actual 3D clipboard mesh object
    public GameObject boardObject;
    // local position when clipboard is fully visible at 80 degrees
    public Vector3 visiblePosition = new Vector3(0f, -0.4f, 0.6f);
    public Vector3 visibleRotation = new Vector3(30f, 0f, 0f);
    // local position when clipboard is hidden below view
    public Vector3 hiddenPosition = new Vector3(0f, -1.5f, 0.6f);
    public Vector3 hiddenRotation = new Vector3(30f, 0f, 0f);

    // clipboard starts appearing at this camera angle
    public float startShowAngle = 65f;
    // clipboard is fully visible at this angle
    public float fullyVisibleAngle = 80f;
    public float smoothSpeed = 10f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Tab key to instantly look at clipboard
    public KeyCode tabKey = KeyCode.Tab;
    // how long the snap animation takes
    public float snapDuration = 0.3f;

    private GameObject cardViewObject;
    private bool hasCard = false;
    private bool tabActive = false;
    private bool isSnapping = false;
    // 0 = clipboard, 1 = card
    private int currentTabItem = 0;

    void Start()
    {
        if (boardObject != null && playerCamera != null)
        {
            // attach clipboard to camera so it moves with player view
            boardObject.transform.SetParent(playerCamera);
            boardObject.transform.localPosition = hiddenPosition;
            boardObject.transform.localRotation = Quaternion.Euler(hiddenRotation);
            // remove physics from clipboard since its now attached to camera
            Rigidbody rb = boardObject.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);
            Collider col = boardObject.GetComponent<Collider>();
            if (col != null) col.enabled = false;
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

        if (!tabActive)
        {
            // first tab press - snap camera down to see clipboard
            tabActive = true;
            StartCoroutine(SnapCamera(fullyVisibleAngle));
        }
        else if (angle >= fullyVisibleAngle - 5f)
        {
            if (hasCard)
            {
                // switch between clipboard and card view
                currentTabItem = (currentTabItem + 1) % 2;
                boardObject.SetActive(currentTabItem == 0);
                if (cardViewObject != null)
                    cardViewObject.SetActive(currentTabItem == 1);
            }
            else
            {
                // no card, hide clipboard and snap camera back up
                tabActive = false;
                StartCoroutine(SnapCamera(0f));
            }
        }
    }

    // called by KeyCard.PickUp() when player picks up the card
    public void OnCardPickedUp(GameObject cardView)
    {
        hasCard = true;
        cardViewObject = cardView;
        // hide card view until player switches to it with Tab
        if (cardViewObject != null) cardViewObject.SetActive(false);
    }

    // KeyCardReader checks this to know if player has the card
    public bool HasCard() => hasCard;

    void UpdateTransform()
    {
        float cv = slideCurve.Evaluate(CalculateSlide(GetAngle()));
        boardObject.transform.localPosition = Vector3.Lerp(
            boardObject.transform.localPosition,
            Vector3.Lerp(hiddenPosition, visiblePosition, cv),
            Time.deltaTime * smoothSpeed);
        boardObject.transform.localRotation = Quaternion.Slerp(
            boardObject.transform.localRotation,
            Quaternion.Lerp(Quaternion.Euler(hiddenRotation), Quaternion.Euler(visibleRotation), cv),
            Time.deltaTime * smoothSpeed);
    }

    float GetAngle()
    {
        // get camera X rotation and normalize to -90 to 90 range
        float r = playerCamera.localEulerAngles.x;
        if (r > 180f) r -= 360f;
        return Mathf.Clamp(r, -90f, 90f);
    }

    float CalculateSlide(float a)
    {
        if (a < startShowAngle) return 0f;
        if (a >= fullyVisibleAngle) return 1f;
        return (a - startShowAngle) / (fullyVisibleAngle - startShowAngle);
    }

    IEnumerator SnapCamera(float target)
    {
        isSnapping = true;
        float start = GetAngle();
        float elapsed = 0f;
        while (elapsed < snapDuration)
        {
            elapsed += Time.deltaTime;
            playerCamera.localRotation = Quaternion.Euler(
                Mathf.Lerp(start, target, elapsed / snapDuration), 0f, 0f);
            yield return null;
        }
        playerCamera.localRotation = Quaternion.Euler(target, 0f, 0f);
        isSnapping = false;
    }
}