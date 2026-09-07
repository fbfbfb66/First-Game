using System;
using UnityEngine;
[Serializable]
public sealed class CombatActionTransition
{
    [SerializeField] private CombatActionCondition[] conditions;
    [SerializeField] private CombatActionCommandKey command;
    [SerializeField] private CombatActionDefinition targetAction;


    public CombatActionCommandKey Command => command;
    public CombatActionDefinition TargetAction => targetAction;

    public bool AreConditionsMet(in CombatActionContext context)
    {
        if (conditions == null || conditions.Length == 0) return true;
        foreach (var condition in conditions)
        {
            if(condition == null)
            {
                Debug.LogWarning("condition is null");
                return false;
            }
            if (condition.IsMet(context) == false) return false;
        }
        return true;
    }
}
