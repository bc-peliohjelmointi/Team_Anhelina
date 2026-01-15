using UnityEngine;

public class CarController : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 5f;
    public float rotationSpeed = 5f;

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
        direction.y = 0;

        transform.position += direction.normalized * speed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (direction.magnitude < 0.5f)
        {
            currentPoint = (currentPoint + 1) % waypoints.Length;
        }
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
        animator.SetTrigger("Crash");
    }
}
