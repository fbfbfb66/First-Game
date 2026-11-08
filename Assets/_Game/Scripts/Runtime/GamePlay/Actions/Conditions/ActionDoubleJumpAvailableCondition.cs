using UnityEngine;

[CreateAssetMenu(fileName = "Condition_DoubleJumpAvailable", menuName = "Game/Actions/Conditions/Double Jump Available")]
public sealed class ActionDoubleJumpAvailableCondition : ActionCondition
{
    public override bool IsMet(in ActionContext context)
    {
        return !context.IsGrounded && context.CanDoubleJump;
    }
}
