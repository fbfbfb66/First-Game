using UnityEngine;
[CreateAssetMenu(fileName ="Action_",menuName ="Game/Combat/Action Definition")]
public sealed class CombatActionDefinition : ScriptableObject
{
    [SerializeField] private CombatActionCommandKey command;
    [SerializeField] private CombatActionStage stage;
    [SerializeField] private CombatActionTransition transition;
    [SerializeField] private CombatActionCondition[] entryConditions;


    public CombatActionCommandKey Command => command;
    public CombatActionStage Stage => stage;
    public CombatActionTransition Transition => transition;

    public bool AreEntryConditionsMet(in CombatActionContext context)
    {
        if(entryConditions == null || entryConditions.Length == 0) return true;
        foreach(var condition in entryConditions)
        {
            if (condition == null)
            {
                Debug.LogWarning($"{Stage.AnimatorStateName} condition is wrong");
                return false;
            }
            if (condition.IsMet(context) == false) return false;
        }
        return true;
    }
}
