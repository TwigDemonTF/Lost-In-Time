using System.Collections;
using UnityEngine;

public class TimelineUIController : MonoBehaviour
{
    [SerializeField] private Animator hourglassAnimator;
    [SerializeField] private float animationDuration = 0.4f;

    private bool isInFuture = false;
    private bool isAnimating = false;

    public bool IsAnimating => isAnimating;

    public IEnumerator PlayTimelineAnimation()
    {
        if (isAnimating)
            yield break;

        isAnimating = true;

        if (!isInFuture)
        {
            hourglassAnimator.SetTrigger("ToFuture");
        }
        else
        {
            hourglassAnimator.SetTrigger("ToPast");
        }

        yield return new WaitForSeconds(animationDuration);

        isInFuture = !isInFuture;
        isAnimating = false;
    }
}