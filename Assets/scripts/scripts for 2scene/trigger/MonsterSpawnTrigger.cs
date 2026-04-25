using UnityEngine;
using System.Collections;

public class MonsterSpawnTrigger : MonoBehaviour
{
    [Header("Monster Settings")]
    public GameObject monsterPrefab;
    public Transform spawnPoint;
    public float visibleTime = 3f;

    [Header("Peek Animation")]
    public float moveOutDistance = 1.5f;
    public float moveOutDuration = 2f;
    public float moveBackDuration = 0.5f;

    [Header("Sound")]
    public AudioSource sound;

    [Header("Behaviour")]
    public bool destroyAfterUse = true;

    [Header("Animation")]
    public string walkBackTrigger = "WalkBack"; // Название триггера для анимации назад

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

        GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation * Quaternion.Euler(0f, 0f, 0f));
        Animator anim = monster.GetComponent<Animator>();

        if (sound != null)
            sound.Play();

        Vector3 startPos = spawnPoint.position;
        Vector3 peekPos = spawnPoint.position + spawnPoint.forward * moveOutDistance;

        // Выход вперёд — Walk играет автоматически через Entry
        yield return StartCoroutine(MoveMonster(monster, startPos, peekPos, moveOutDuration, EaseOut));

        // Стоит на виду
        yield return new WaitForSeconds(visibleTime);

        if (sound != null)
            sound.Stop();

        // Активируем триггер — переключает на WalkBack
        if (anim != null)
            anim.SetTrigger(walkBackTrigger);

        // Уходит назад
        yield return StartCoroutine(MoveMonster(monster, peekPos, startPos, moveBackDuration, EaseIn));

        if (monster != null)
            Destroy(monster);

        if (destroyAfterUse)
            Destroy(gameObject);
    }

    IEnumerator MoveMonster(GameObject monster, Vector3 from, Vector3 to, float duration, System.Func<float, float> easing)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (monster == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = easing(t);

            monster.transform.position = Vector3.Lerp(from, to, easedT);
            yield return null;
        }

        if (monster != null)
            monster.transform.position = to;
    }

    float EaseOut(float t) => 1f - Mathf.Pow(1f - t, 3f);
    float EaseIn(float t) => t * t * t;
}