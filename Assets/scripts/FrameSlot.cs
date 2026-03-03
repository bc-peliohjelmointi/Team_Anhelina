using UnityEngine;

public class FrameSlot : MonoBehaviour
{
    public int slotIndex;
    public int standNumber;
    public int correctFrameNumber;

    private DraggableObject currentPaper;

    private void OnTriggerEnter(Collider other)
    {
        DraggableObject obj = other.GetComponent<DraggableObject>();
        if (obj != null && obj.isPaper && currentPaper == null)
        {
            currentPaper = obj;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentPaper == null)
        {
            DraggableObject obj = other.GetComponent<DraggableObject>();
            if (obj != null && obj.isPaper)
            {
                currentPaper = obj;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DraggableObject obj = other.GetComponent<DraggableObject>();
        if (obj != null && currentPaper == obj)
        {
            currentPaper = null;
        }
    }

    public bool HasCorrectFrame()
    {
        if (currentPaper == null)
        {
            return false;
        }

        return currentPaper.frameNumber == correctFrameNumber;
    }

    public bool HasPaper()
    {
        return currentPaper != null;
    }

    public int GetCurrentFrameNumber()
    {
        if (currentPaper != null)
        {
            return currentPaper.frameNumber;
        }
        return -1;
    }
}