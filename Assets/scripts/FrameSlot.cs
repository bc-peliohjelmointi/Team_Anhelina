using UnityEngine;

public class FrameSlot : MonoBehaviour
{
    public int slotIndex;
    public int standNumber;
    public DraggableObject currentPaper;

    private void OnTriggerEnter(Collider other)
    {
        DraggableObject obj = other.GetComponent<DraggableObject>();
        if (obj != null && obj.isPaper && currentPaper == null)
        {
            currentPaper = obj;
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

    public int GetFrameNumber()
    {
        if (currentPaper != null)
        {
            return currentPaper.frameNumber;
        }
        return -1;
    }

    public bool HasPaper()
    {
        return currentPaper != null;
    }
}