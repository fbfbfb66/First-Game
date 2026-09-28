using System.Collections.Generic;
public sealed class CombatActionUsageRuntime
{
    private readonly HashSet<CombatActionDefinition> usedActions = new();

    public void Record(CombatActionDefinition action)
    {
        if (action == null) return;
        usedActions.Add(action);
    }

    public bool HasBeenUsed(CombatActionDefinition action)
    {
        if (action == null) return false;
        return usedActions.Contains(action);
    }

    public void Clear()
    {
        usedActions.Clear();
    }
}
