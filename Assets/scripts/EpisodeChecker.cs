using UnityEngine;

public class EpisodeChecker : MonoBehaviour
{
    public FrameSlot[] stand1Slots = new FrameSlot[5];
    public FrameSlot[] stand2Slots = new FrameSlot[5];
    public FrameSlot[] stand3Slots = new FrameSlot[5];

    private int[] episode1Order = { 1, 2, 3, 4, 5 };
    private int[] episode2Order = { 6, 7, 8, 9, 10 };
    private int[] episode3Order = { 11, 12, 13, 14, 15 };

    public bool IsEpisode1Correct()
    {
        return CheckOrder(stand1Slots, episode1Order);
    }

    public bool IsEpisode2Correct()
    {
        return CheckOrder(stand2Slots, episode2Order);
    }

    public bool IsEpisode3Correct()
    {
        return CheckOrder(stand3Slots, episode3Order);
    }

    private bool CheckOrder(FrameSlot[] slots, int[] correctOrder)
    {
        if (slots.Length != correctOrder.Length)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || !slots[i].HasPaper())
                return false;

            int frameNumber = slots[i].GetFrameNumber();
            if (frameNumber != correctOrder[i])
            {
                return false;
            }
        }
        return true;
    }
}