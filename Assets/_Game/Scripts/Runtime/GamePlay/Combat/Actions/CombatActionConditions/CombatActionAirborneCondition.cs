using UnityEngine;

[CreateAssetMenu(
    fileName = "Condition_Airborne_",
    menuName = "Game/Combat/Conditions/Airborne")]
public sealed class CombatActionAirborneCondition : CombatActionCondition
{
    public override bool IsMet(in CombatActionContext context)
    {
        return context.IsGrounded == false;
    }
}
