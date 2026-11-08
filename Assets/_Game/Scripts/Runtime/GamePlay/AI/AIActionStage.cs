using System;
using UnityEngine;

[Serializable]
public sealed class AIActionStage : ActionStage
{
    [SerializeField] private MotionStageEffect[] effects = System.Array.Empty<MotionStageEffect>();
    public override MotionStageEffect[] Effects => effects;

    [SerializeField] private DisplacementRule aiDisplacement = new();
    public override DisplacementRule AIDisplacement => aiDisplacement;
}
