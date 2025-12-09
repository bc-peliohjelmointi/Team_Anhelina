using UnityEngine;

public class AutoDistanceCulling : MonoBehaviour
{
    public float maxDistance = 100f;
    private Camera cam;
    private Renderer[] allRenderers;

    void Start()
    {
        cam = Camera.main;

        // Новий метод — без попереджень
        allRenderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (cam == null) return;

        Vector3 camPos = cam.transform.position;

        foreach (var rend in allRenderers)
        {
            if (rend == null) continue;

            float dist = Vector3.Distance(camPos, rend.transform.position);
            rend.enabled = dist < maxDistance;
        }
    }
}
