using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f; // movement speed

    void Update()
    {
        // get input axes
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // create movement vector
        Vector3 movement = new Vector3(moveX, 0f, moveZ);

        // move player
        transform.Translate(movement * speed * Time.deltaTime);
    }
}