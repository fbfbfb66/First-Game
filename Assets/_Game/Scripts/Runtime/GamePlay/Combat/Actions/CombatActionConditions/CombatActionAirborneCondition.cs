using UnityEngine;

[CreateAssetMenu(
    fileName = "Condition_Airborne_",
    menuName = "Game/Actions/Combat/Conditions/Airborne")]
public sealed class CombatActionAirborneCondition : ActionCondition
{
    public override bool IsMet(in ActionContext context)
    {
        return context.IsGrounded == false;
    }
}
