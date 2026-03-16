using UnityEngine;

public class MonologueTrigger : MonoBehaviour
{
    public Playermonologue monologue;
    public string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        monologue.StartMonologue();
        gameObject.SetActive(false); // один раз
    }
}