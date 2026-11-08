using UnityEngine;
using System;
[Serializable]
public class CombatActionStage : ActionStage
{
    [SerializeField] private CombatHitWindowDefinition[] hitWindows;

    public virtual float HorizontalDeceleration => 0f;

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
