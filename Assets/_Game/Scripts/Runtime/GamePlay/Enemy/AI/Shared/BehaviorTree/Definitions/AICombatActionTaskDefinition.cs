using UnityEngine;
[CreateAssetMenu(fileName = "AICombatTaskDefinition", menuName = "Game/AI/Enemies/Tasks/Combat")]
public class AICombatActionTaskDefinition : AITaskDefinition
{
    [SerializeField] private CombatActionDefinition action;
    public override AITask CreateTask(in AIAgentContext context)
    {
        if(action == null || context.CombatExecutor == null)
        {
            Debug.LogWarning($"[EnemyCombat] {context.Owner.name}: action or combat executor is null; cannot create combat action task.", context.Owner);
            return null;
        }

        return new AICombatActionTask(context, action);
    }
}
