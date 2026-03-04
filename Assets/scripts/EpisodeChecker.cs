using UnityEngine;

public class EpisodeChecker : MonoBehaviour
{
    public FrameSlot[] stand1Slots = new FrameSlot[5];
    public FrameSlot[] stand2Slots = new FrameSlot[5];
    public FrameSlot[] stand3Slots = new FrameSlot[5];

    void Start()
    {
        for (int i = 0; i < stand1Slots.Length; i++)
        {
            if (stand1Slots[i] != null)
            {
                stand1Slots[i].correctFrameNumber = i + 1;
            }
        }

        for (int i = 0; i < stand2Slots.Length; i++)
        {
            if (stand2Slots[i] != null)
            {
                stand2Slots[i].correctFrameNumber = i + 6;
            }
        }

        for (int i = 0; i < stand3Slots.Length; i++)
        {
            if (stand3Slots[i] != null)
            {
                stand3Slots[i].correctFrameNumber = i + 11;
            }
        }
    }

    public bool IsEpisode1Correct()
    {
        return CheckStand(stand1Slots);
    }

    public bool IsEpisode2Correct()
    {
        return CheckStand(stand2Slots);
    }

    public bool IsEpisode3Correct()
    {
        return CheckStand(stand3Slots);
    }

    private bool CheckStand(FrameSlot[] slots)
    {
        if (slots == null) return false;

        foreach (FrameSlot slot in slots)
        {
            if (slot == null || !slot.HasPaper() || !slot.HasCorrectFrame())
            {
                return false;
            }
        }

        return true;
    }
}