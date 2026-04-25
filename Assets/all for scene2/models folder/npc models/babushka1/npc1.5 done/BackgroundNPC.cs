using System.Collections;
using UnityEngine;

public class BackgroundNPC : MonoBehaviour
{
    [Header("Components")]
    public Animator animator;
    public AudioSource ambientAudio;

    [Header("Animator Trigger Names")]
    public string talkingTrigger = "Resume";   // анимация по умолчанию (сидит и говорит)
    public string quietTrigger = "Quiet";      // анимация (просто сидит)

    // Вызвать из NPCInteraction когда начинается диалог
    public void OnDialogueStart()
    {
        if (animator != null)
            animator.SetTrigger(quietTrigger);

        if (ambientAudio != null)
            ambientAudio.Stop();
    }

    // Вызвать из NPCInteraction когда диалог заканчивается или скипается
    public void OnDialogueEnd()
    {
        if (animator != null)
            animator.SetTrigger(talkingTrigger);

        if (ambientAudio != null)
            ambientAudio.Play();
    }
}