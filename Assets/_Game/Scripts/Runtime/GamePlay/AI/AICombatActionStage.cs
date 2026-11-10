using System;
using UnityEngine;

[Serializable]
public sealed class AICombatActionStage : CombatActionStage
{
    [SerializeField] private AICombatDisplacementDirection displacementDirection;
    public AICombatDisplacementDirection DisplacementDirection => displacementDirection;

    [SerializeField] private DisplacementRule aiDisplacement = new();
    public override DisplacementRule AIDisplacement => aiDisplacement;
}

public enum AICombatDisplacementDirection
{
    Facing,
    TowardsTarget
}
