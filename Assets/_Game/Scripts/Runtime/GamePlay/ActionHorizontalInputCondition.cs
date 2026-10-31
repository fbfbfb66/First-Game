using UnityEngine;

[CreateAssetMenu(fileName = "Condition_HorizontalInput", menuName = "Game/Actions/Conditions/Horizontal Input")]
public sealed class ActionHorizontalInputCondition : ActionCondition
{
    [SerializeField] private bool requireHorizontalInput = true;

    public override bool IsMet(in ActionContext context)
    {
        bool hasHorizontalInput = !Mathf.Approximately(context.MoveInput.x, 0f);
        return hasHorizontalInput == requireHorizontalInput;
    }
}
