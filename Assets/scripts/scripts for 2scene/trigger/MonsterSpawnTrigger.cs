using UnityEngine;
using System.Collections;

public class MonsterSpawnTrigger : MonoBehaviour
{
    [Header("Monster Settings")]
    public GameObject monsterPrefab; // Hirviön prefab, josta luodaan pelästysefekti
    public Transform spawnPoint;     // Sijainti, johon hirviö ilmestyy
    public float visibleTime = 3f;   // Kuinka kauan hirviö on näkyvissä sekunteissa

    [Header("Sound")]
    public AudioSource sound; // Äänikomponentti hirviön ilmestymiseen

    [Header("Behaviour")]
    public bool destroyAfterUse = true; // Jos true, poistetaan trigger hirviön katoamisen jälkeen

    private bool triggered = false; // Onko trigger jo lauennut

    private void OnTriggerEnter(Collider other)
    {
        // Jos trigger on jo lauennut, ei tehdä mitään
        if (triggered) return;

        // Käynnistetään hirviön ilmestyminen, kun pelaaja astuu alueelle
        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(SpawnMonster());
        }
    }

    IEnumerator SpawnMonster()
    {
        // Jos hirviön prefab tai ilmestymispiste puuttuu, lopetetaan
        if (monsterPrefab == null || spawnPoint == null)
            yield break;

        // Luodaan hirviö määritettyyn sijaintiin ja suuntaan
        GameObject monster = Instantiate(
            monsterPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Toistetaan pelästysääni
        if (sound != null)
            sound.Play();

        // Odotetaan niin kauan kuin hirviö on näkyvissä
        yield return new WaitForSeconds(visibleTime);

        // Pysäytetään ääni hirviön katoamisen jälkeen
        if (sound != null)
            sound.Stop();

        // Poistetaan hirviö scenestä
        if (monster != null)
            Destroy(monster);

        // Poistetaan trigger-objekti, jos se on asetettu kertakäyttöiseksi
        if (destroyAfterUse)
            Destroy(gameObject);
    }
}