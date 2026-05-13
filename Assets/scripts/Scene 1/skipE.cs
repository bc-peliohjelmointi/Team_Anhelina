using UnityEngine;
using System.Collections;

public class SkipE : MonoBehaviour
{
    public GameObject skipUI;
    public Transform npc;
    public float hideDistance = 18f;
    public BOSSInteract bossInteract;

    public Transform player;


    void Start()
    {

        if (skipUI != null) skipUI.SetActive(false);

    }




    void Update()
    {
        if (player == null) return;
        if (skipUI == null) return;


        float dist = Vector3.Distance(npc.position, player.position);


        bool voicePlaying = bossInteract != null &&
                           (bossInteract.onLine1 || bossInteract.onLine2) &&
                           dist <= hideDistance;

        skipUI.SetActive(voicePlaying);
    }

    void OnDrawGizmosSelected()
    {
        if (npc == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(npc.position, hideDistance);
    }
}