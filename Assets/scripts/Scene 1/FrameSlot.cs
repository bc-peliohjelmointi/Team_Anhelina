using UnityEngine;

// represents one slot on a stand where a paper frame can be placed
// uses overlap sphere to detect which paper is currently in this slot
public class FrameSlot : MonoBehaviour
{
    public int slotIndex;
    public int standNumber;
    public int correctFrameNumber; // set by EpisodeChecker in Start
    public float detectionRadius = 0.3f; // how close paper needs to be to count

    // finds the nearest paper inside detection radius
    public DraggableObject GetPaperInSlot()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius);
        DraggableObject closestPaper = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            DraggableObject obj = col.GetComponent<DraggableObject>();
            if (obj != null && obj.isPaper)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPaper = obj; // keep track of closest one only
                }
            }
        }

        return closestPaper;
    }

    // checks if the paper in this slot has the right frame number
    public bool HasCorrectFrame()
    {
        DraggableObject paper = GetPaperInSlot();
        if (paper == null) return false;
        return paper.frameNumber == correctFrameNumber;
    }

    public bool HasPaper()
    {
        return GetPaperInSlot() != null;
    }

    // returns -1 if slot is empty
    public int GetCurrentFrameNumber()
    {
        DraggableObject paper = GetPaperInSlot();
        if (paper != null) return paper.frameNumber;
        return -1;
    }

    // yellow sphere shows detection range, green dot shows slot center
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}