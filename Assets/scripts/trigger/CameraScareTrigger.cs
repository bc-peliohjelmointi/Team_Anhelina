using UnityEngine;
using System.Collections;

public class CameraScareTrigger : MonoBehaviour
{
    public Camera playerCamera;
    public Camera scareCamera;
    public float scareDuration = 3f;

    public AudioSource breathingSound; 

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(ScareSequence(other));
        }
    }

    IEnumerator ScareSequence(Collider player)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        movement.LockControl();
        playerCamera.gameObject.SetActive(false);
        scareCamera.gameObject.SetActive(true);

        if (breathingSound != null)
            breathingSound.Play();

        yield return new WaitForSeconds(scareDuration);

        if (breathingSound != null)
            breathingSound.Stop();

        scareCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        movement.UnlockControl();
    }
}