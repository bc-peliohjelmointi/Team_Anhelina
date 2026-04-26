using UnityEngine;

public class FinishPoint : MonoBehaviour
{
    public GameObject monster;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            monster.SetActive(false);
            Debug.Log("Player saved!");
        }
    }
}