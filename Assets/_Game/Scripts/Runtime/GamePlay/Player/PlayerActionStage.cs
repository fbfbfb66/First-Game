using System;
using UnityEngine;

[Serializable]
public sealed class PlayerActionStage : ActionStage
{
    [SerializeField] private MotionStageEffect[] effects = System.Array.Empty<MotionStageEffect>();
    public override MotionStageEffect[] Effects => effects;

    [SerializeField] private ActionEntryVelocityRule entryVelocityRule = new();
    public override ActionEntryVelocityRule EntryVelocityRule => entryVelocityRule;
}
