using UnityEngine;

// checks if the frames on each stand are in the correct order
// stand 1 = frames 1-5, stand 2 = frames 6-10, stand 3 = frames 11-15
public class EpisodeChecker : MonoBehaviour
{
    public FrameSlot[] stand1Slots = new FrameSlot[5];
    public FrameSlot[] stand2Slots = new FrameSlot[5];
    public FrameSlot[] stand3Slots = new FrameSlot[5];

    void Start()
    {
        // assign correct frame numbers automatically based on position in array
        for (int i = 0; i < stand1Slots.Length; i++)
        {
            if (stand1Slots[i] != null)
            {
                stand1Slots[i].correctFrameNumber = i + 1; // 1,2,3,4,5
            }
        }

        for (int i = 0; i < stand2Slots.Length; i++)
        {
            if (stand2Slots[i] != null)
            {
                stand2Slots[i].correctFrameNumber = i + 6; // 6,7,8,9,10
            }
        }

        for (int i = 0; i < stand3Slots.Length; i++)
        {
            if (stand3Slots[i] != null)
            {
                stand3Slots[i].correctFrameNumber = i + 11; // 11,12,13,14,15
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

    // returns true only if every slot has a paper AND its the right frame number
    private bool CheckStand(FrameSlot[] slots)
    {
        if (slots == null) return false;

        foreach (FrameSlot slot in slots)
        {
            if (slot == null || !slot.HasPaper() || !slot.HasCorrectFrame())
            {
                return false; // any wrong slot = episode is wrong
            }
        }

        return true;
    }
}