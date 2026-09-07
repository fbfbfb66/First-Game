using UnityEngine;

public abstract class CombatActionCondition : ScriptableObject
{
    public abstract bool IsMet(in CombatActionContext context);
}
