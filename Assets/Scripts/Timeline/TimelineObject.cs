using UnityEngine;

public class TimelineObject : MonoBehaviour
{
    public bool existsInPast = true;
    public bool existsInFuture = false;
    private FutureObject futureObject;
    private bool isDestroyed = false;

    void Start()
    {
        futureObject = GetComponent<FutureObject>();

        UpdateState(TimelineManager.Instance.currentTimeline);
        TimelineManager.Instance.OnTimelineChanged += UpdateState;
    }

    void UpdateState(TimelineManager.Timeline timeline)
    {
        if (isDestroyed)
        {
            gameObject.SetActive(false);
            return;
        }

        bool shouldExist =
            (timeline == TimelineManager.Timeline.Past && existsInPast) ||
            (timeline == TimelineManager.Timeline.Future && existsInFuture);

        gameObject.SetActive(shouldExist);
    }

    public void DestroyObject()
    {
        isDestroyed = true;
        UpdateState(TimelineManager.Instance.currentTimeline);
    }
}