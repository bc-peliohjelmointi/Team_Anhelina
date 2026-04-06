using UnityEngine;

public class FallingBricksZone : MonoBehaviour
{
    public GameObject brickPrefab;     // Tiili-prefab, josta luodaan putoavat tiilet
    public float spawnInterval = 0.5f; // Tiilien ilmestymisväli sekunteissa
    public int brickCount = 10;        // Kuinka monta tiiltä putoaa yhteensä
    public float spawnHeight = 10f;    // Kuinka korkealta tiilet putoavat alueen yläpuolelta

    private bool playerInside = false; // Onko pelaaja alueella tällä hetkellä
    private Collider zoneCollider;     // Alueen törmäyskomponentti

    private void Start()
    {
        // Haetaan alueen törmäyskomponentti
        zoneCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Aloitetaan tiilien putoaminen, kun pelaaja astuu alueelle
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            StartCoroutine(SpawnBricks());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Merkitään, että pelaaja on poistunut alueelta
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private Vector3 GetRandomPointInCollider()
    {
        // Arvotaan satunnainen X- ja Z-sijainti alueen rajojen sisältä
        Vector3 point = new Vector3(
            Random.Range(zoneCollider.bounds.min.x, zoneCollider.bounds.max.x),
            zoneCollider.bounds.max.y + spawnHeight, // Luodaan tiili alueen yläpuolelle
            Random.Range(zoneCollider.bounds.min.z, zoneCollider.bounds.max.z)
        );
        return point;
    }

    private System.Collections.IEnumerator SpawnBricks()
    {
        int spawned = 0;

        // Luodaan tiilejä niin kauan kuin pelaaja on alueella ja tiiliä on jäljellä
        while (playerInside && spawned < brickCount)
        {
            // Haetaan satunnainen sijainti alueen yläpuolelta
            Vector3 spawnPos = GetRandomPointInCollider();

            // Luodaan uusi tiili valittuun sijaintiin ilman kiertymistä
            Instantiate(brickPrefab, spawnPos, Quaternion.identity);

            spawned++;

            // Odotetaan ennen seuraavan tiilen luomista
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}