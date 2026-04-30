using UnityEngine;

public class InteractPrompt : MonoBehaviour
{
    public GameObject promptUI;
    public Transform npc;
    public float showDistance = 3f;
    public BOSSInteract bossInteract;

    private Transform player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        promptUI.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(npc.position, player.position);

        // ẩn F khi đang interact hoặc cooldown
        if (bossInteract != null && (bossInteract.isInteracting || bossInteract.onCooldown))
        {
            promptUI.SetActive(false);
            return;
        }

        if (dist <= showDistance)
            promptUI.SetActive(true);
        else
            promptUI.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (npc == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(npc.position, showDistance);
    }
}