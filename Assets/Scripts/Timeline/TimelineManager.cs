using UnityEngine;
using System;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance;

    public enum Timeline
    {
        Past,
        Future
    }

    public Timeline currentTimeline = Timeline.Past;

    public event Action<Timeline> OnTimelineChanged;

    void Awake()
    {
        Instance = this;
    }

    public void SwitchTimeline()
    {
        currentTimeline = (currentTimeline == Timeline.Past) ? Timeline.Future : Timeline.Past;

        OnTimelineChanged?.Invoke(currentTimeline);
    }
}