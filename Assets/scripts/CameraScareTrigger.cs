using UnityEngine;
using System.Collections;

public class CameraScareTrigger : MonoBehaviour
{
    public Camera playerCamera;
    public Camera scareCamera;
    public float scareDuration = 3f;

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

        // Блокируем управление
        movement.LockControl();

        // Переключаем камеры
        playerCamera.gameObject.SetActive(false);
        scareCamera.gameObject.SetActive(true);

        yield return new WaitForSeconds(scareDuration);

        // Возвращаем всё обратно
        scareCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        movement.UnlockControl();
    }
}