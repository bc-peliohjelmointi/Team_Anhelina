using UnityEngine;
using System.Collections;

public class MonsterSpawnTrigger : MonoBehaviour
{
    [Header("Monster Settings")]
    public GameObject monsterPrefab;
    public Transform spawnPoint;
    public float visibleTime = 3f;

    [Header("Behaviour")]
    public bool destroyAfterUse = true;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;

        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(SpawnMonster());
        }
    }

    IEnumerator SpawnMonster()
    {
        if (monsterPrefab == null || spawnPoint == null)
            yield break;

        GameObject monster = Instantiate(
            monsterPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        yield return new WaitForSeconds(visibleTime);

        if (monster != null)
            Destroy(monster);

        if (destroyAfterUse)
            Destroy(gameObject);
    }
}