using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    public int missionID;
    public MissionSystem missionSystem;

    bool playerNear = false;

    void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (missionSystem.GetCurrentMission() == missionID)
            {
                Talk();
                missionSystem.CompleteMission(missionID);
            }
        }
    }

    void Talk()
    {
        Debug.Log("Talking to NPC: " + gameObject.name);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}