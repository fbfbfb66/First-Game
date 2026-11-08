using UnityEngine;

[CreateAssetMenu(fileName = "Condition_SkillUnlocked", menuName = "Game/Actions/Conditions/Skill Unlocked")]
public sealed class ActionSkillUnlockedCondition : ActionCondition
{
    [SerializeField] private SkillData skill;

    public override bool IsMet(in ActionContext context)
    {
        return context.IsSkillUnlocked(skill);
    }

    public override bool IsMet(in CombatActionContext context)
    {
        return context.IsSkillUnlocked(skill);
    }
}
