using UnityEngine;

public class TimelineObject : MonoBehaviour
{
    public bool existsInPast = true;
    public bool existsInFuture = false;
    private FutureObject futureObject;

    void Start()
    {
        futureObject = GetComponent<FutureObject>();

        UpdateState(TimelineManager.Instance.currentTimeline);
        TimelineManager.Instance.OnTimelineChanged += UpdateState;
    }

    void UpdateState(TimelineManager.Timeline timeline)
    {
        bool shouldExist =
            (timeline == TimelineManager.Timeline.Past && existsInPast) ||
            (timeline == TimelineManager.Timeline.Future && existsInFuture);

        if (futureObject != null && futureObject.IsActivated)
        {
            shouldExist = false;
        }

        gameObject.SetActive(shouldExist);
        Debug.Log($"Updating {gameObject.name}, shouldExist: {shouldExist}");
    }

    void OnDestroy()
    {
        if (TimelineManager.Instance != null)
            TimelineManager.Instance.OnTimelineChanged -= UpdateState;
    }
}