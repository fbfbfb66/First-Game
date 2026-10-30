using UnityEngine;

// Static action data shared by player, enemy and boss action definitions.
// Execution and per-character runtime state belong to the action executor.
public abstract class ActionDefinition : ScriptableObject
{
    [SerializeField] private ActionCommandKey command;
    [SerializeField] private ActionCondition[] entryConditions;

    public ActionCommandKey Command => command;
    public ActionCondition[] ActionEntryConditions => entryConditions;
    public abstract ActionStage Stage { get; }

    public bool AreEntryConditionsMet(in ActionContext context)
    {
        if (entryConditions == null || entryConditions.Length == 0) return true;
        foreach (var condition in entryConditions)
        {
            if (condition == null)
            {
                Debug.LogWarning($"[Action] {name}: an entry condition is missing; entry rejected.", this);
                return false;
            }
            if (!condition.IsMet(in context)) return false;
        }
        return true;
    }

    public bool AreEntryConditionsMet(in CombatActionContext context)
    {
        if (entryConditions == null || entryConditions.Length == 0) return true;
        foreach (var condition in entryConditions)
        {
            if (condition == null)
            {
                Debug.LogWarning($"[Action] {name}: an entry condition is missing; entry rejected.", this);
                return false;
            }
            if (!condition.IsMet(in context)) return false;
        }
        return true;
    }
}
