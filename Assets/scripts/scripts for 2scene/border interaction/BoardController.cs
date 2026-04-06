using UnityEngine;

public class BoardController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;      
    public GameObject boardObject;      // Laudan peliobjekti
    public RectTransform boardCanvas;   

    [Header("Visible State (80 degrees)")]
    // Laudan sijainti, kun se on täysin näkyvissä
    public Vector3 visiblePosition = new Vector3(0, -0.4f, 0.6f);
    // Laudan kulma, kun se on täysin näkyvissä
    public Vector3 visibleRotation = new Vector3(30f, 0f, 0f);

    [Header("Hidden State (below 65 degrees)")]
    // Laudan sijainti, kun se on piilotettu
    public Vector3 hiddenPosition = new Vector3(0, -1.5f, 0.6f);
    // Laudan kulma, kun se on piilotettu
    public Vector3 hiddenRotation = new Vector3(30f, 0f, 0f);

    [Header("Camera Angle Settings")]
    // Kamerakulma, josta lauta alkaa näkyä
    public float startShowAngle = 65f;
    // Kamerakulma, jossa lauta on täysin näkyvissä
    public float fullyVisibleAngle = 80f;

    [Header("Animation")]
    // Animaation pehmeyden nopeus
    public float smoothSpeed = 10f;
    // Animaatiokäyrä laudan liikkeelle
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    void Start()
    {
        // Tarkistetaan, että lauta ja kamera ovat olemassa
        if (boardObject != null && playerCamera != null)
        {
            // Asetetaan lauta kameran lapsiobjektiksi, jotta se seuraa kameraa
            boardObject.transform.SetParent(playerCamera);

            // Tarkistetaan, että kanvas on olemassa
            if (boardCanvas != null)
            {
                // Asetetaan kanvas laudan lapsiobjektiksi
                boardCanvas.SetParent(boardObject.transform);
                // Siirretään kanvas hieman laudan eteen, jotta se näkyy
                boardCanvas.localPosition = new Vector3(0f, 0f, 0.01f);
                // Nollataan kankaan kiertyminen
                boardCanvas.localRotation = Quaternion.identity;
                // Skaalataan kanvas pieneksi, jotta se sopii 3D-maailmaan
                boardCanvas.localScale = new Vector3(0.002f, 0.002f, 0.002f);
                // Asetetaan kankaan koko oletuksena
                boardCanvas.sizeDelta = new Vector2(1000, 1000);
            }

            // Haetaan laudan renderöijäkomponentti
            Renderer boardRenderer = boardObject.GetComponent<Renderer>();

            // Jos renderöijä ja kanvas ovat olemassa, sovitetaan kanvas laudan kokoon
            if (boardRenderer != null && boardCanvas != null)
            {
                // Haetaan laudan rajat maailmakoordinaateissa
                Bounds bounds = boardRenderer.bounds;
                float width = bounds.size.x;   // Laudan leveys
                float height = bounds.size.y;  // Laudan korkeus

                // Siirretään kanvas laudan pintaan
                boardCanvas.localPosition = new Vector3(0, 0, 0.01f);
                // Sovitetaan kankaan koko vastaamaan laudan kokoa
                boardCanvas.sizeDelta = new Vector2(width * 1000f, height * 1000f);
            }
        }
    }

    void Update()
    {
        // Jos kamera tai lauta puuttuu, ei tehdä mitään
        if (playerCamera == null || boardObject == null) return;

        // Päivitetään laudan sijainti ja kulma joka ruutu
        UpdateBoardTransform();
    }

    void UpdateBoardTransform()
    {
        // Haetaan kameran nykyinen pystysuuntainen kulma
        float cameraXRotation = GetCameraXRotation();

        // Jos pelaaja pitää Tab-näppäintä pohjassa, näytetään lauta kokonaan
        if (Input.GetKey(KeyCode.Tab))
        {
            cameraXRotation = fullyVisibleAngle;
        }

        // Lasketaan laudan liukumisarvo kamerakulman perusteella (0 = piilotettu, 1 = näkyvissä)
        float slideValue = CalculateSlideValue(cameraXRotation);

        // Sovelletaan animaatiokäyrää pehmeämmän liikkeen saamiseksi
        float curvedValue = slideCurve.Evaluate(slideValue);

        // Lasketaan laudan kohdeesijainti interpoloimalla piilotetun ja näkyvän välillä
        Vector3 targetPosition = Vector3.Lerp(hiddenPosition, visiblePosition, curvedValue);

        // Lasketaan laudan kohdekiertyminen interpoloimalla piilotetun ja näkyvän välillä
        Quaternion targetRotation = Quaternion.Lerp(
            Quaternion.Euler(hiddenRotation),
            Quaternion.Euler(visibleRotation),
            curvedValue
        );

        // Liikutetaan lauta pehmeästi kohdesijaintiin
        boardObject.transform.localPosition = Vector3.Lerp(
            boardObject.transform.localPosition,
            targetPosition,
            Time.deltaTime * smoothSpeed
        );

        // Kierretään lauta pehmeästi kohdesuuntaan (Slerp on parempi kulmille kuin Lerp)
        boardObject.transform.localRotation = Quaternion.Slerp(
            boardObject.transform.localRotation,
            targetRotation,
            Time.deltaTime * smoothSpeed
        );
    }

    float GetCameraXRotation()
    {
        // Haetaan kameran paikallinen X-kulma (0–360 astetta)
        float rotation = playerCamera.localEulerAngles.x;

        // Muutetaan kulma välille -180...180, jotta alaspäin katsominen on negatiivista
        if (rotation > 180f)
        {
            rotation -= 360f;
        }

        // Rajataan kulma välille -90...90 astetta
        return Mathf.Clamp(rotation, -90f, 90f);
    }

    float CalculateSlideValue(float cameraAngle)
    {
        // Jos kamera osoittaa liian alas, lauta on täysin piilotettu
        if (cameraAngle < startShowAngle)
        {
            return 0f;
        }
        // Jos kamera osoittaa tarpeeksi ylös, lauta on täysin näkyvissä
        else if (cameraAngle >= fullyVisibleAngle)
        {
            return 1f;
        }
        // Muuten lasketaan liukumisarvo kulmien väliltä
        else
        {
            return (cameraAngle - startShowAngle) / (fullyVisibleAngle - startShowAngle);
        }
    }
}