using System;

public sealed class StateCancelTransitionRule
{
    public EntityState TargetState { get; }
    public int RequiredWindowId { get; }

    public StateCancelTransitionRule(EntityState targetState,int requiredWindowId = -1)
    {
        if (targetState == null) throw new ArgumentNullException(nameof(targetState));
        if(requiredWindowId < -1) throw new ArgumentOutOfRangeException(nameof(requiredWindowId));
        TargetState = targetState;
        RequiredWindowId = requiredWindowId;
    }

    public bool Allows(EntityState requestedTarget, int activeWindowId)
    {
        if(requestedTarget != TargetState) return false;
        if (RequiredWindowId == -1) return true;
        if (activeWindowId != RequiredWindowId) return false;
        return true;
    }
}
