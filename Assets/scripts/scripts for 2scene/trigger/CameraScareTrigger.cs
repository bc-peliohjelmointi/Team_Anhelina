using UnityEngine;
using System.Collections;

public class CameraScareTrigger : MonoBehaviour
{
    public Camera playerCamera;       // Pelaajan normaali kamera
    public Camera scareCamera;        // Pelästyskamera, joka aktivoituu hetkeksi
    public float scareDuration = 3f;  // Kuinka kauan pelästyskohtaus kestää sekunteissa
    public AudioSource breathingSound;// Hengitysääni pelästyskohtauksen aikana

    private bool triggered = false; // Onko pelästys jo lauennut

    private void OnTriggerEnter(Collider other)
    {
        // Jos pelästys on jo lauennut, ei tehdä mitään
        if (triggered) return;

        // Tarkistetaan, onko alueelle astunut objekti pelaaja
        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(ScareSequence(other));
        }
    }

    IEnumerator ScareSequence(Collider player)
    {
        // Haetaan pelaajan liikkumiskomponentti
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        // Lukitaan pelaajan liikkuminen pelästyskohtauksen ajaksi
        movement.LockControl();

        // Vaihdetaan normaalikamera pelästyskameraan
        playerCamera.gameObject.SetActive(false);
        scareCamera.gameObject.SetActive(true);

        // Aloitetaan hengitysääni
        if (breathingSound != null)
            breathingSound.Play();

        // Odotetaan pelästyskohtauksen keston verran
        yield return new WaitForSeconds(scareDuration);

        // Pysäytetään hengitysääni
        if (breathingSound != null)
            breathingSound.Stop();

        // Palataan takaisin normaalikameraan
        scareCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);

        // Palautetaan pelaajan liikkuminen
        movement.UnlockControl();
    }
}