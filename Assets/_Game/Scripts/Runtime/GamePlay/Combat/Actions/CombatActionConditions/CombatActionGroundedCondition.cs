using UnityEngine;
[CreateAssetMenu(
    fileName = "Condition_Grounded_",
    menuName = "Game/Combat/Conditions/Grounded")]
public sealed class CombatActionGroundedCondition : CombatActionCondition
{
    [SerializeField] private bool requiredGrounded = true;
    public bool RequiredGrounded => requiredGrounded;
    public override bool IsMet(in CombatActionContext context)
    {
        if (context.IsGrounded != requiredGrounded) return false;
        return true;
    }
}
