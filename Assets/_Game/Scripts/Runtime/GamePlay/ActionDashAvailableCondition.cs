using UnityEngine;

[CreateAssetMenu(fileName = "Condition_DashAvailable", menuName = "Game/Actions/Conditions/Dash Available")]
public sealed class ActionDashAvailableCondition : ActionCondition
{
    public override bool IsMet(in ActionContext context)
    {
        return context.CanDash;
    }
}
