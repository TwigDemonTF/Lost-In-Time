using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    public bool isActivated = false;

    [Header("Target")]
    public FutureObject target; // what this affects

    public void Interact()
    {
        if (isActivated) return;

        if (TimelineManager.Instance.currentTimeline != TimelineManager.Timeline.Past)
            return;

        isActivated = true;

        target?.Activate();
    }
}