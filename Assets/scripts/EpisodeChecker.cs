using UnityEngine;

public class EpisodeChecker : MonoBehaviour
{
    public FrameSlot[] stand1Slots = new FrameSlot[5];
    public FrameSlot[] stand2Slots = new FrameSlot[5];
    public FrameSlot[] stand3Slots = new FrameSlot[5];

    void Start()
    {
        SetupCorrectFrameNumbers();
    }

    void SetupCorrectFrameNumbers()
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
        if (slots == null || slots.Length == 0)
        {
            return false;
        }

        foreach (FrameSlot slot in slots)
        {
            if (slot == null)
            {
                return false;
            }

            if (!slot.HasPaper())
            {
                return false;
            }

            if (!slot.HasCorrectFrame())
            {
                return false;
            }
        }

        return true;
    }

    public void DebugCheckEpisode(int episodeNumber)
    {
        FrameSlot[] slots = null;

        if (episodeNumber == 1) slots = stand1Slots;
        else if (episodeNumber == 2) slots = stand2Slots;
        else if (episodeNumber == 3) slots = stand3Slots;

        if (slots == null) return;

        Debug.Log($"=== Episode {episodeNumber} Check ===");

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                int currentFrame = slots[i].GetCurrentFrameNumber();
                int correctFrame = slots[i].correctFrameNumber;
                bool isCorrect = slots[i].HasCorrectFrame();

                Debug.Log($"Slot {i}: Current Frame = {currentFrame}, Correct Frame = {correctFrame}, Is Correct = {isCorrect}");
            }
            else
            {
                Debug.Log($"Slot {i}: NULL");
            }
        }

        bool result = CheckStand(slots);
        Debug.Log($"Episode {episodeNumber} Result: {result}");
    }
}