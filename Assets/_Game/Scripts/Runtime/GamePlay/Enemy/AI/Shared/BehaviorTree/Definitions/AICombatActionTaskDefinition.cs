using UnityEngine;
[CreateAssetMenu(fileName = "AICombatTaskDefinition", menuName = "Game/AI/Enemies/Tasks/Combat")]
public class AICombatActionTaskDefinition : AITaskDefinition
{
    public override AITask CreateTask(in AIAgentContext context)
    {
        if(context.SelectedAction == null || context.CombatExecutor == null)
        {
            Debug.LogWarning($"[EnemyCombat] {context.Owner.name}: action or combat executor is null; cannot create combat action task.", context.Owner);
            return null;
        }
        if(!(context.SelectedAction is CombatActionDefinition combatAction))
        {
            Debug.LogWarning($"[EnemyCombat] {context.Owner.name}: selected action is not a CombatActionDefinition; cannot create combat action task.", context.Owner);
            return null;
        }

        return new AICombatActionTask(context,combatAction);
    }
}
