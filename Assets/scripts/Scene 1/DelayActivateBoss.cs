using UnityEngine;

public class DelayActivateBoss : MonoBehaviour
{
    public GameObject[] objectsToActivate;
    public float delay = 5f;

    void Start()
    {
        foreach (GameObject obj in objectsToActivate)
            if (obj != null) obj.SetActive(false);

        Invoke(nameof(Activate), delay);
    }

    void Activate()
    {
        foreach (GameObject obj in objectsToActivate)
            if (obj != null) obj.SetActive(true);
    }
}