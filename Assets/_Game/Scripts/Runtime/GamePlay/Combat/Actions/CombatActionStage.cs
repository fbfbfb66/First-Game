using UnityEngine;
using System;
[Serializable]
public sealed class CombatActionStage : ActionStage
{
    [SerializeField] private CombatActionEntryVelocityRule entryVelocityRule = new();
    [SerializeField, Min(0f)] private float horizontalDeceleration;
    [SerializeField] private CombatHitWindowDefinition[] hitWindows;

    public CombatActionEntryVelocityRule EntryVelocityRule => entryVelocityRule;
    public float HorizontalDeceleration => horizontalDeceleration;

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
