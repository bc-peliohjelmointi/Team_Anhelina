using System.Collections;
using UnityEngine;

public class MonsterLookTrigger : MonoBehaviour
{
    public Transform player;
    public Transform monster;
    public MonoBehaviour playerController;

    public float lookTime = 2f;
    public float rotateSpeed = 3f;

    private bool activated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activated) return;

        if (other.CompareTag("Player"))
        {
            activated = true;
            StartCoroutine(LookAtMonster());
        }
    }

    IEnumerator LookAtMonster()
    {
        if (playerController != null)
            playerController.enabled = false;

        monster.gameObject.SetActive(true);

        Vector3 dir = monster.position - player.position;
        dir.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * rotateSpeed;
            player.rotation = Quaternion.Slerp(player.rotation, targetRotation, t);
            yield return null;
        }

        yield return new WaitForSeconds(lookTime);

        if (playerController != null)
            playerController.enabled = true;
    }
}