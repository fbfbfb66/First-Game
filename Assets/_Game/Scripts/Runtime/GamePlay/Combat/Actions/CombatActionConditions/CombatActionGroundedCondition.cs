using UnityEngine;
[CreateAssetMenu(
    fileName = "Condition_Grounded_",
    menuName = "Game/Combat/Conditions/Grounded")]
public sealed class CombatActionGroundedCondition : CombatActionCondition
{
    public override bool IsMet(in CombatActionContext context)
    {
        return context.CanEnterGrounded;
    }
}
