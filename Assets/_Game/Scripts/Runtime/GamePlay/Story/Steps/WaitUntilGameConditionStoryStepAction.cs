using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(
    fileName = "WaitUntilGameConditionStoryStep",
    menuName = "Game/Story/Step/Wait Until Game Condition")]
public class WaitUntilGameConditionStoryStepAction : StoryStepAction
{
    [SerializeField] private List<GameCondition> conditions = new();

    [Tooltip("If true, all conditions must be met. If false, any one condition being met is enough.")]
    [SerializeField] private bool requireAllConditions = true;

    public override IEnumerator Execute(StoryContext context)
    {
        if (conditions == null || conditions.Count == 0)
        {
            yield break;
        }
        GameConditionContext conditionContext = new GameConditionContext(context.FlagCenter,context.QuestManager,
            context.Runner == null ? null : context.Runner.gameObject,context.Instigator);

        while (!IsConditionGroupMet(conditionContext))
        {
            yield return null;
        }
    }

    private bool IsConditionGroupMet(GameConditionContext context)
    {
        if(requireAllConditions) return AreAllConditionsMet(context);
        else return IsAnyConditionMet(context);
    }

    private bool AreAllConditionsMet(GameConditionContext context)
    {
        foreach(var condition in conditions)
        {
            if(condition == null) return false;
            if(!condition.IsMet(context)) return false;
        }
        return true;
    }
    private bool IsAnyConditionMet(GameConditionContext context)
    {
        foreach(var condition in conditions)
        {
            if(condition == null) continue;
            if(condition.IsMet(context)) return true;
        }
        return false;
    }
}
