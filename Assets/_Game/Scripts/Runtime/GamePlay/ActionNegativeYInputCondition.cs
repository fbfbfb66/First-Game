using UnityEngine;

[CreateAssetMenu(fileName = "Condition_NegativeYInput", menuName = "Game/Actions/Conditions/Negative Y Input")]
public sealed class ActionNegativeYInputCondition : ActionCondition
{
    public override bool IsMet(in ActionContext context)
    {
        return context.MoveInput.y < 0f;
    }
}
