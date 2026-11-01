public class EnemyCombatActionController : CombatActionController
{
    // EnemyCombatExecutor starts AI-selected actions and plays their stages.
    // The executor checks completion without an enemy FSM or input buffer.
    public bool TryStartAction(CombatActionDefinition action, in CombatActionContext context)
    {
        if (!TryPrepareEntryAction(action, in context)) return false;
        BeginAction(action);
        return true;
    }
}
