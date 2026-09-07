using UnityEngine;
using System;
[Serializable]
public sealed class CombatActionStage
{
    [SerializeField] private string animatorStateName;
    [SerializeField] private CombatActionCancelWindowDefinition[] cancelWindows;
    [SerializeField] private CombatHitWindowDefinition[] hitWindows;

    public int AnimatorStateHash => Animator.StringToHash(animatorStateName);
    public string AnimatorStateName => animatorStateName;

    public bool TryGetCancelWindow(int windowId,out CombatActionCancelWindowDefinition cancelWindow)
    {
        cancelWindow = null;
        if (cancelWindows == null || cancelWindows.Length == 0) return false;
        foreach(var window in cancelWindows)
        {
            if (window == null) continue;
            if(window.WindowId == windowId)
            {
                cancelWindow = window;
                return true;
            }
        }
        return false;
    }

    public bool TryGetHitWindow(int windowId, out CombatHitWindowDefinition hitWindow)
    {
        hitWindow = null;
        if (hitWindows == null || hitWindows.Length == 0) return false;
        foreach(var current in hitWindows)
        {
            if (current == null) continue;
            if(windowId == current.WindowId)
            {
                hitWindow = current;
                return true;
            }
        }
        return false;
    }
}
