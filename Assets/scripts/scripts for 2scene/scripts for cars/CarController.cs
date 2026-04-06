using UnityEngine;

public class CarController : MonoBehaviour
{
    public Transform[] waypoints;    // Reittipisteet, joita auto seuraa järjestyksessä
    public float speed = 5f;         // Auton liikkumisnopeus
    public float rotationSpeed = 5f; // Kuinka nopeasti auto kääntyy kohti seuraavaa pistettä
    public float stopDistance = 0.5f;// Etäisyys, jolla auto siirtyy seuraavaan reittipisteeseen

    private int currentPoint = 0;    // Nykyisen reittipisteen indeksi
    private bool isStopped = false;  // Onko auto pysähtynyt
    private Animator animator;       // Animaattori törmäysanimaatiota varten

    void Start()
    {
        // Haetaan animaattorikomponentti tästä objektista
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Jos auto on pysähtynyt tai reittipisteitä ei ole, ei liikuta
        if (isStopped || waypoints.Length == 0)
            return;

        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        // Lasketaan suunta nykyiseen reittipisteeseen
        Vector3 direction = waypoints[currentPoint].position - transform.position;
        direction.y = 0f; // Ei liikuta ylös tai alas

        // Jos auto on tarpeeksi lähellä reittipistettä, siirrytään seuraavaan
        if (direction.magnitude < stopDistance)
        {
            // % waypoints.Length varmistaa, että reitti toistuu loputtomasti
            currentPoint = (currentPoint + 1) % waypoints.Length;
            return;
        }

        // Lasketaan kohdesuunta reittipisteeseen
        // +90 astetta korjaa mallin suunnan, jos se osoittaa sivulle
        Quaternion targetRotation = Quaternion.LookRotation(
            direction,
            Vector3.up
        ) * Quaternion.Euler(0f, 90f, 0f);

        // Käännetään auto pehmeästi kohdesuuntaan
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // Liikutetaan autoa eteenpäin sen omaan vasempaan suuntaan
        transform.position += -transform.right * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Pysäytetään auto, jos se törmää pelaajaan
        if (collision.gameObject.CompareTag("Player"))
        {
            StopCar();
        }
    }

    void StopCar()
    {
        isStopped = true;

        // Käynnistetään törmäysanimaatio, jos animaattori on olemassa
        if (animator != null)
            animator.SetTrigger("Crash");
    }
}