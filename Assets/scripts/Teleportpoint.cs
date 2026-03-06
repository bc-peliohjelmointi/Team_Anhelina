using UnityEngine;

public class Teleportpoint : MonoBehaviour
{
    public BoardPickup boardPickup;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (boardPickup != null && !boardPickup.HasBoard())
            {
                return;
            }

            LevelManager.Instance.LoadScene("scene 2", "CrossFade");
        }
    }
}