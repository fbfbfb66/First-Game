using System;
using UnityEngine;
[Serializable]
public sealed class CombatActionTransition
{
    [SerializeField] private CombatActionCommandKey command;
    [SerializeField] private CombatActionDefinition targetAction;


    public CombatActionCommandKey Command => command;
    public CombatActionDefinition TargetAction => targetAction;
}
