using UnityEngine;

public class ScreamerTrigger : MonoBehaviour
{
    [Header("Setup")]
    public GameObject monsterPrefab;
    public string playerTag = "Player";

    [Header("Spawn In Front Of Player")]
    [Tooltip("На каком расстоянии перед игроком появится монстр")]
    public float spawnDistance = 20f;

    [Tooltip("Высота спавна относительно игрока (0 = на уровне глаз)")]
    public float spawnHeightOffset = 0f;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        triggered = true;

        // Берём направление взгляда камеры (только горизонталь)
        Camera cam = Camera.main;
        Vector3 forward = cam != null ? cam.transform.forward : other.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        // Позиция спавна — прямо перед игроком
        Vector3 spawnPos = other.transform.position
                         + forward * spawnDistance
                         + Vector3.up * spawnHeightOffset;

        // Монстр сразу смотрит на игрока
        Quaternion spawnRot = Quaternion.LookRotation(-forward);

        GameObject monster = Instantiate(monsterPrefab, spawnPos, spawnRot);

        ScreamerMonster sm = monster.GetComponent<ScreamerMonster>();
        if (sm != null)
            sm.Activate(other.transform);

        gameObject.SetActive(false);
    }
}