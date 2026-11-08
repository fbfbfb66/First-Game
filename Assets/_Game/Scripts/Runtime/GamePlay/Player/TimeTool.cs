using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class TimeTool
{
    private readonly Dictionary<string, float> deadlines = new();

    public void StartTimeCounter(float time, string timerId)
    {
        if (string.IsNullOrWhiteSpace(timerId)) throw new ArgumentException("Timer ID is required.", nameof(timerId));
        if (time < 0f || float.IsNaN(time) || float.IsInfinity(time)) throw new ArgumentOutOfRangeException(nameof(time));
        deadlines[timerId] = Time.time + time;
    }

    public bool IsRunning(string timerId)
    {
        if (!deadlines.TryGetValue(timerId, out float deadline)) return false;
        if (Time.time < deadline) return true;
        deadlines.Remove(timerId);
        return false;
    }
}
