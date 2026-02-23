using UnityEngine;

public class CarController_3 : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float rotationSpeed = 5f;
    public float stopDistance = 0.5f;

    private int currentPoint = 0;
    private bool isStopped = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isStopped || waypoints.Length == 0)
            return;

        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        Vector3 direction = waypoints[currentPoint].position - transform.position;
        direction.y = 0f;

        if (direction.magnitude < stopDistance)
        {
            currentPoint = (currentPoint + 1) % waypoints.Length;
            return;
        }

        // Поворот к цели
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.Euler(0f, 180f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Движение к цели
        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentPoint].position, speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StopCar();
        }
    }

    void StopCar()
    {
        isStopped = true;

        if (animator != null)
            animator.SetTrigger("Crash");
    }
}
