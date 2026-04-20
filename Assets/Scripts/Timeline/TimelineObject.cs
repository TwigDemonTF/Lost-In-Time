using UnityEngine;

public class TimelineObject : MonoBehaviour
{
    public bool existsInPast = true;
    public bool existsInFuture = false;

    private void Start()
    {
        UpdateState(TimelineManager.Instance.currentTimeline);
        TimelineManager.Instance.OnTimelineChanged += UpdateState;
    }

    private void OnDestroy()
    {
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.OnTimelineChanged -= UpdateState;
        }
    }

    void UpdateState(TimelineManager.Timeline timeline)
    {
        bool shouldExist =
            (timeline == TimelineManager.Timeline.Past && existsInPast) ||
            (timeline == TimelineManager.Timeline.Future && existsInFuture);

        gameObject.SetActive(shouldExist);
    }
}