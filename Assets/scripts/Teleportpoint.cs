using UnityEngine;

public class Teleportpoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if(collision.CompareTag("Player"))
        {
            // Teleport the player to the next scene
            SceneController.instance.NextLevel();

        }
    }
}
