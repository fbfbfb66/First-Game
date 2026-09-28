using UnityEngine;
[CreateAssetMenu(
    fileName = "Condition_Action Not Used",
    menuName = "Game/Combat/Conditions/Action Not Used")]
public class CombatActionNotUsedCondition : CombatActionCondition
{
    [SerializeField] private CombatActionDefinition action;
    public override bool IsMet(in CombatActionContext context)
    {
        if (action == null)
        {
            return false;
        }

        return !context.HasActionBeenUsed(action);
    }
}
