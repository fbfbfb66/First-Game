using UnityEngine;
[CreateAssetMenu(
    fileName = "Condition_Grounded_",
    menuName = "Game/Actions/Combat/Conditions/Grounded")]
public sealed class CombatActionGroundedCondition : ActionCondition
{
    public override bool IsMet(in ActionContext context)
    {
        return context.CanEnterGrounded;
    }
}
