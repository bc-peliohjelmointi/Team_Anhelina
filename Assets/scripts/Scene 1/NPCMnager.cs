using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public GameObject npc1;
    public GameObject npc2;

    void Update()
    {
        // khi npc1 đã idle xong thì swap
        if (npc1 != null && !npc1.activeSelf) return;

        BossIntroSequence boss = npc1.GetComponent<BossIntroSequence>();
        if (boss != null && boss.IsFinished)
        {
            npc1.SetActive(false);
            npc2.SetActive(true);
        }
    }
}