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
        if (!float.IsFinite(horizontalDirection)) return false;
        return TryStart(definition, horizontalDirection < 0f ? Vector2.left : Vector2.right);
    }

    public bool TryStart(DisplacementRule definition, Vector2 direction)
    {
        if (definition == null || !definition.IsValid || definition.Distance == Vector2.zero ||
            !float.IsFinite(direction.x) || !float.IsFinite(direction.y) || direction.sqrMagnitude <= Mathf.Epsilon) return false;
        rule = definition;
        // X is distance along the supplied direction; Y remains additional world vertical distance.
        offset = direction.normalized * definition.Distance.x + Vector2.up * definition.Distance.y;
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
