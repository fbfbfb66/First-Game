using System;
using UnityEngine;

[Serializable]
public class ActionStage
{
    [SerializeField] private string animatorStateName;
    [SerializeField] private CombatActionCancelWindowDefinition[] cancelWindows;

    public string AnimatorStateName => animatorStateName;
    public int AnimatorStateHash => Animator.StringToHash(animatorStateName);

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
}
