// MonsterCameraTrigger.cs
using System.Collections;
using UnityEngine;

public class MonsterCameraTrigger : MonoBehaviour
{
    [Header("Камеры")]
    public Camera playerCam;
    public Camera monsterCam;

    [Header("Монстр")]
    public GameObject monster;
    public float viewDuration = 2.5f;

    [Header("Настройки")]
    public float spawnDistanceBehind = 2.5f;
    public bool triggerOnce = true;

    private bool _triggered = false;

    private void Start()
    {
        if (monster != null)
            monster.SetActive(false);

        playerCam.enabled = true;
        monsterCam.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && _triggered) return;

        _triggered = true;
        StartCoroutine(MonsterLookSequence(other.transform));
    }

    private IEnumerator MonsterLookSequence(Transform player)
    {
        // Спавним монстра сзади игрока
        Vector3 behindPlayer = player.position - player.forward * spawnDistanceBehind;
        behindPlayer.y = player.position.y;

        monster.transform.position = behindPlayer;
        monster.transform.LookAt(player.position);
        monster.SetActive(true);

        // Переключаем камеру
        playerCam.enabled = false;
        monsterCam.enabled = true;

        yield return new WaitForSeconds(viewDuration);

        // Возвращаем камеру игрока
        monsterCam.enabled = false;
        playerCam.enabled = true;

        yield return new WaitForSeconds(0.3f);
        monster.SetActive(false);
    }
}