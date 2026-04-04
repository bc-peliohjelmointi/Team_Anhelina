using System.Collections;
using UnityEngine;
using DG.Tweening;

public class CrossFade : SceneTransition
{
    public CanvasGroup crossFade; // slide show from scene to scene

    public override IEnumerator AnimateTransitionIn()
    {
        var tweener = crossFade.DOFade(1f, 1f); // fade to black
        yield return tweener.WaitForCompletion(); // wait until done
    }

    public override IEnumerator AnimateTransitionOut()
    {
        var tweener = crossFade.DOFade(0f, 1f); // fade back to visible
        yield return tweener.WaitForCompletion(); // wait until done
    }
}