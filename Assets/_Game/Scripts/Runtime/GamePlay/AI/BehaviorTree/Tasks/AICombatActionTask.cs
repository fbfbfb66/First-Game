using UnityEngine;

public class AICombatActionTask : AITask
{
    private AIAgentContext context;
    private CombatActionDefinition action;
    private EnemyCombatExecutor executor;
    private GroundSensor groundSensor;
    public AICombatActionTask(in AIAgentContext context,CombatActionDefinition action)
    {
        if(context.CombatExecutor == null)
        {
            string ownerName = context.Owner != null ? context.Owner.name : "<missing owner>";
            throw new System.ArgumentException($"[EnemyCombat] {ownerName}: context has no combat executor; cannot create combat action task.", nameof(context));
        }

        this.context = context;
        this.action = action;
        executor = context.CombatExecutor;
    }

    protected override AITaskStatus OnStart()
    {
        if(context.Owner == null || context.CombatExecutor == null)
        {
            string ownerName = context.Owner != null ? context.Owner.name : "<missing owner>";
            Debug.LogWarning($"[EnemyCombat] {ownerName}: context owner or combat executor is missing; cannot start combat action task.", context.Owner);
            return AITaskStatus.Failed;
        }
        if (context.Target == null || context.Target == context.Owner ||
            !context.Target.gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[EnemyCombat] {context.Owner.name}: target is missing, inactive or the owner itself; attack not started.", context.Owner);
            return AITaskStatus.Failed;
        }
        EnemyCombatActionController controller = context.CombatExecutor.GetComponent<EnemyCombatActionController>();
        groundSensor = context.Owner.GetComponent<GroundSensor>();
        if(controller == null)
        {
            Debug.LogWarning($"[EnemyCombat] {context.Owner.name}: combat action controller is missing; attack not started.", context.Owner);
            return AITaskStatus.Failed;
        }
        CombatActionContext combatContext;
        if(groundSensor == null)
        {
            combatContext = new CombatActionContext(false, false, executor.transform.position, controller.ChainRuntime, controller.UsageRuntime);
        }
        else combatContext = new CombatActionContext(groundSensor.IsGrounded, groundSensor.CanEnterGrounded, executor.transform.position, controller.ChainRuntime, controller.UsageRuntime);
        if (executor.TryStartAction(action, in combatContext))
        {
            return AITaskStatus.Running;
        }
        else
        {
            return AITaskStatus.Failed;
        }
    }


    protected override AITaskStatus OnTick(float deltaTime)
    {
        if(Status != AITaskStatus.Running) return Status;
        executor.Tick();
        if(executor.Status == CombatExecutionStatus.NotStarted) return AITaskStatus.Failed;
        if(executor.Status == CombatExecutionStatus.Running) return AITaskStatus.Running;
        if(executor.Status == CombatExecutionStatus.Succeeded) return AITaskStatus.Succeeded;
        if(executor.Status == CombatExecutionStatus.Cancelled) return AITaskStatus.Failed;
        return AITaskStatus.Failed;
    }


    protected override void OnEnd(bool interrupted)
    {
        if(executor.Status == CombatExecutionStatus.Running)
        {
            executor.CancelCurrentExecution();
        }
    }
}
