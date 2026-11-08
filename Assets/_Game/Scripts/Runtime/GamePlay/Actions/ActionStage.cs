using System;
using UnityEngine;

[Serializable]
public class ActionStage
{
    [SerializeField] private string animatorStateName;
    // Runtime accessors; actor-specific subclasses own the serialized movement settings.
    public virtual ActionEntryVelocityRule EntryVelocityRule => null;
    public virtual DisplacementRule AIDisplacement => null;
    [SerializeField] private ActionCondition[] conditions;
    [SerializeField] private CombatActionCancelWindowDefinition[] cancelWindows;

    public virtual MotionStageEffect[] Effects => System.Array.Empty<MotionStageEffect>();

    public string AnimatorStateName => animatorStateName;
    public int AnimatorStateHash => Animator.StringToHash(animatorStateName);

    public bool AreConditionsMet(in ActionContext context)
    {
        if (conditions == null) return true;
        foreach (var condition in conditions)
        {
            if (condition == null)
            {
                Debug.LogWarning($"[ActionStage] {animatorStateName}: a condition is missing; stage rejected.");
                return false;
            }
            if (!condition.IsMet(in context)) return false;
        }
        return true;
    }

    public bool AreConditionsMet(in CombatActionContext context)
    {
        if (conditions == null) return true;
        foreach (var condition in conditions)
        {
            if (condition == null)
            {
                Debug.LogWarning($"[ActionStage] {animatorStateName}: a condition is missing; stage rejected.");
                return false;
            }
            if (!condition.IsMet(in context)) return false;
        }
        return true;
    }

    public bool TryGetCancelWindow(int windowId, out CombatActionCancelWindowDefinition cancelWindow)
    {
        cancelWindow = null;
        if (cancelWindows == null || cancelWindows.Length == 0) return false;
        foreach (var window in cancelWindows)
        {
            if (window == null || window.WindowId != windowId) continue;
            cancelWindow = window;
            return true;
        }
        return false;
    }


    public bool AreConditionsMet(in AIActionContext context)
    {
        if (conditions == null) return true;
        foreach (var condition in conditions)
        {
            if (condition == null)
            {
                Debug.LogWarning($"[ActionStage] {animatorStateName}: a condition is missing; stage rejected.");
                return false;
            }
            if (!condition.IsMet(in context)) return false;
        }
        return true;
    }
}
