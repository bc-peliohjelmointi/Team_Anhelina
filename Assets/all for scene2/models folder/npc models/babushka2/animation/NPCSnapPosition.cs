using UnityEngine;

public class NPCSnapPosition : MonoBehaviour
{
    public Transform hipsbone;
    public Animator animator;
    private float savedHipsX;

    void Start()
    {
        savedHipsX = hipsbone.localPosition.x;
    }

    void LateUpdate()
    {
        AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
        float targetZ = hipsbone.localPosition.z;
        float progress = current.normalizedTime % 1f; // прогресс от 0 до 1

        if (current.IsName("Sit"))
        {
            targetZ = 0.035f;
        }
        else if (current.IsName("StandIdle") || current.IsName("Walk"))
        {
            targetZ = 0.28f;
        }
        else if (current.IsName("StandUp"))
        {
            // Первая половина: 0.035 → плавно до 0.25
            targetZ = Mathf.Lerp(0.035f, 0.25f, progress);
        }
        else if (current.IsName("SitDown"))
        {
            // Первая половина: 0.28 → плавно до 0.035
            targetZ = Mathf.Lerp(0.28f, 0.035f, progress);
        }

        hipsbone.localPosition = new Vector3(
            savedHipsX,
            hipsbone.localPosition.y,
            targetZ
        );
    }
}