using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform player;        // Đối tượng player mà enemy đuổi theo
    public float speed = 2f;        // Tốc độ chạy
    public float stoppingDistance = 1.0f; // Khoảng cách dừng lại
    public float rotationSpeed = 10f;     // Tốc độ xoay mượt

    void Start()
    {
        // Nếu chưa gắn Player trong Inspector, tự tìm object tên "Player"
        if (player == null)
        {
            GameObject p = GameObject.Find("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Tính hướng đến Player
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // Bỏ trục Y để không bay lên/xuống

        float distance = direction.magnitude;

        if (distance > stoppingDistance)
        {
            // Di chuyển lại gần Player
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime
            );

            // Xoay mặt về hướng Player
            if (direction != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    look,
                    rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}