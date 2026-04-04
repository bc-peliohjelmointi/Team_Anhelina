using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition; // store start pos

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // random offset
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0); // apply shake

            elapsed += Time.deltaTime;
            yield return null; // wait next frame
        }

        transform.localPosition = originalPos; // reset position
    }
}