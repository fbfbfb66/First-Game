using System;
using UnityEngine;

[Serializable]
public sealed class AICombatActionStage : CombatActionStage
{
    [SerializeField] private DisplacementRule aiDisplacement = new();
    public override DisplacementRule AIDisplacement => aiDisplacement;
}
