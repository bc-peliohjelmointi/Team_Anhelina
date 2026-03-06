using UnityEngine;

public class FrameSlot : MonoBehaviour
{
    public int slotIndex;
    public int standNumber;
    public int correctFrameNumber;
    public float detectionRadius = 0.3f;

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
                    closestPaper = obj;
                }
            }
        }

        return closestPaper;
    }

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

    public int GetCurrentFrameNumber()
    {
        DraggableObject paper = GetPaperInSlot();
        if (paper != null) return paper.frameNumber;
        return -1;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.05f);
    }
}