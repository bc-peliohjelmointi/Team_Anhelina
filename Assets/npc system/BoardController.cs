using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public GameObject boardObject;
    public RectTransform boardCanvas;

    [Header("Visible State (80 degrees)")]
    public Vector3 visiblePosition = new Vector3(0, -0.4f, 0.6f);
    public Vector3 visibleRotation = new Vector3(30f, 0f, 0f);

    [Header("Hidden State (below 65 degrees)")]
    public Vector3 hiddenPosition = new Vector3(0, -1.5f, 0.6f);
    public Vector3 hiddenRotation = new Vector3(30f, 0f, 0f);

    [Header("Camera Angle Settings")]
    public float startShowAngle = 65f;
    public float fullyVisibleAngle = 80f;

    [Header("Animation")]
    public float smoothSpeed = 10f;
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    void Start()
    {
        if (boardObject != null && playerCamera != null)
        {
            boardObject.transform.SetParent(playerCamera);

            if (boardCanvas != null)
            {
                boardCanvas.SetParent(boardObject.transform);

                boardCanvas.localPosition = new Vector3(0f, 0f, 0.01f);
                boardCanvas.localRotation = Quaternion.identity;

                boardCanvas.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                boardCanvas.sizeDelta = new Vector2(1000, 1000);
            }

            Renderer boardRenderer = boardObject.GetComponent<Renderer>();

            if (boardRenderer != null && boardCanvas != null)
            {
                Bounds bounds = boardRenderer.bounds;

                float width = bounds.size.x;
                float height = bounds.size.y;

                boardCanvas.localPosition = new Vector3(0, 0, 0.01f);

                boardCanvas.sizeDelta = new Vector2(width * 1000f, height * 1000f);
            }
        }
    }

    void Update()
    {
        if (playerCamera == null || boardObject == null) return;

        UpdateBoardTransform();
    }

    void UpdateBoardTransform()
    {
        float cameraXRotation = GetCameraXRotation();
        float slideValue = CalculateSlideValue(cameraXRotation);
        float curvedValue = slideCurve.Evaluate(slideValue);

        Vector3 targetPosition = Vector3.Lerp(hiddenPosition, visiblePosition, curvedValue);
        Quaternion targetRotation = Quaternion.Lerp(
            Quaternion.Euler(hiddenRotation),
            Quaternion.Euler(visibleRotation),
            curvedValue
        );

        boardObject.transform.localPosition = Vector3.Lerp(
            boardObject.transform.localPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );

        boardObject.transform.localRotation = Quaternion.Slerp(
            boardObject.transform.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    float GetCameraXRotation()
    {
        float rotation = playerCamera.localEulerAngles.x;

        if (rotation > 180f)
        {
            rotation -= 360f;
        }

        return Mathf.Clamp(rotation, -90f, 90f);
    }

    float CalculateSlideValue(float cameraAngle)
    {
        if (cameraAngle < startShowAngle)
        {
            return 0f;
        }
        else if (cameraAngle >= fullyVisibleAngle)
        {
            return 1f;
        }
        else
        {
            return (cameraAngle - startShowAngle) / (fullyVisibleAngle - startShowAngle);
        }
    }
}