using System;
using UnityEngine;

[Serializable]
public sealed class PlayerCombatActionStage : CombatActionStage
{
    [SerializeField] private ActionEntryVelocityRule entryVelocityRule = new();
    [SerializeField, Min(0f)] private float horizontalDeceleration;
    public override ActionEntryVelocityRule EntryVelocityRule => entryVelocityRule;
    public override float HorizontalDeceleration => horizontalDeceleration;
}
