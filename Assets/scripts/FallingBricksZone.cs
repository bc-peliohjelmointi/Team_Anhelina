using UnityEngine;

public class FallingBricksZone : MonoBehaviour
{
    public GameObject brickPrefab;   // Префаб кирпича
    public float spawnInterval = 0.5f; // Интервал падения кирпичей
    public int brickCount = 10;      // Количество кирпичей для спавна
    public float spawnHeight = 10f;  // Высота, с которой падают кирпичи

    private bool playerInside = false;
    private Collider zoneCollider;

    private void Start()
    {
        zoneCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            StartCoroutine(SpawnBricks());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private Vector3 GetRandomPointInCollider()
    {
        Vector3 point = new Vector3(
            Random.Range(zoneCollider.bounds.min.x, zoneCollider.bounds.max.x),
            zoneCollider.bounds.max.y + spawnHeight, // падение сверху
            Random.Range(zoneCollider.bounds.min.z, zoneCollider.bounds.max.z)
        );
        return point;
    }

    private System.Collections.IEnumerator SpawnBricks()
    {
        int spawned = 0;
        while (playerInside && spawned < brickCount)
        {
            Vector3 spawnPos = GetRandomPointInCollider();
            Instantiate(brickPrefab, spawnPos, Quaternion.identity);
            spawned++;
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}