using UnityEngine;

public class MSpawn : MonoBehaviour
{
    public GameObject monster;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            monster.SetActive(true);
        }
    }

}
