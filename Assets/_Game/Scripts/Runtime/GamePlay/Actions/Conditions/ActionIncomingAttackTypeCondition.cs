using UnityEngine;

[CreateAssetMenu(fileName = "Condition_IncomingAttackType", menuName = "Game/Actions/Conditions/Incoming Attack Type")]
public sealed class ActionIncomingAttackTypeCondition : ActionCondition
{
    [SerializeField] private AttackType attackType;
    public AttackType AttackType => attackType;

    public override bool IsMet(in ActionContext context)
    {
        return context.IncomingAttackType.HasValue && context.IncomingAttackType.Value == attackType;
    }
    public override bool IsMet(in AIActionContext context)
    {
        return context.DamageContext.HasValue && context.DamageContext.Value.AttackType == attackType;
    }
}

