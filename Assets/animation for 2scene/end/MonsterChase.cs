using UnityEngine;
using UnityEngine.AI;

public class MonsterChase : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 3f;

    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        agent.speed = moveSpeed;
    }

    void Update()
    {
        agent.speed = moveSpeed; // гарантирует обновление

        if (player != null)
        {
            agent.SetDestination(player.position);
        }
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
        agent.speed = speed;
    }
}