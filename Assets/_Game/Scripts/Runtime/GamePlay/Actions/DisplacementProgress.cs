using UnityEngine;

// Per-actor progress; configuration can be shared without sharing execution state.
public sealed class DisplacementProgress
{
    private DisplacementRule rule;
    private Vector2 offset;
    private float elapsed;
    private float lastProgress;
    public bool IsRunning => rule != null;
    public bool ControlsVertical => offset.y != 0f;

    public bool TryStart(DisplacementRule definition, float horizontalDirection)
    {
        if (definition == null || !definition.IsValid || definition.Distance == Vector2.zero ||
            !float.IsFinite(horizontalDirection)) return false;
        rule = definition;
        offset = new Vector2(definition.Distance.x * (horizontalDirection < 0f ? -1f : 1f), definition.Distance.y);
        elapsed = lastProgress = 0f;
        return true;
    }

    public bool TryAdvance(float deltaTime, out Vector2 delta)
    {
        delta = Vector2.zero;
        if (rule == null || elapsed >= rule.Duration || !float.IsFinite(deltaTime) || deltaTime < 0f) return false;
        elapsed = Mathf.Min(elapsed + deltaTime, rule.Duration);
        float progress = elapsed >= rule.Duration ? 1f : rule.ProgressCurve.Evaluate(elapsed / rule.Duration);
        if (!float.IsFinite(progress)) return false;
        delta = offset * (progress - lastProgress);
        lastProgress = progress;
        return true;
    }

    public void Clear()
    {
        rule = null;
        offset = Vector2.zero;
        elapsed = lastProgress = 0f;
    }
}
