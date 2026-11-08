using System;
using UnityEngine;

[Serializable]
public sealed class DisplacementRule
{
    [SerializeField] private Vector2 distance;
    [SerializeField, Min(0.001f)] private float duration = 0.2f;
    [SerializeField] private AnimationCurve progressCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public Vector2 Distance => distance;
    public float Duration => duration;
    public AnimationCurve ProgressCurve => progressCurve;

    public bool IsValid => float.IsFinite(distance.x) && float.IsFinite(distance.y) &&
        float.IsFinite(duration) && duration > 0f && progressCurve != null && progressCurve.length > 0 &&
        Mathf.Approximately(progressCurve.Evaluate(0f), 0f) && Mathf.Approximately(progressCurve.Evaluate(1f), 1f);
}
