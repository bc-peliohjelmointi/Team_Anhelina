using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    Light lt;

    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 0.05f; 

    float timer;

    void Awake()
    {
        lt = GetComponent<Light>();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            lt.intensity = Random.Range(minIntensity, maxIntensity);
            timer = flickerSpeed;
        }
    }
}