using UnityEngine;

public class AICombatActionTask : AITask
{
    private AIContext context;
    private CombatActionDefinition action;
    private EnemyCombatExecutor executor;
    private GroundSensor groundSensor;
    public AICombatActionTask(in AIContext context,CombatActionDefinition action)
    {
        if(context.combatExecutor == null)
        {
            throw new System.ArgumentException($"[EnemyCombat] {context.Owner.name}: context has no combat executor; cannot create combat action task.", nameof(context));
        }

        this.context = context;
        this.action = action;
        executor = context.combatExecutor;
    }

    protected override AITaskStatus OnStart()
    {
        if(context.Owner == null || context.combatExecutor == null)
        {
            Debug.LogWarning($"[EnemyCombat] {context.Owner.name}: context owner or combat executor is missing; cannot start combat action task.", context.Owner);
            return AITaskStatus.Failed;
        }
        EnemyCombatActionController controller = context.combatExecutor.GetComponent<EnemyCombatActionController>();
        groundSensor = context.Owner.GetComponent<GroundSensor>();
        if(controller == null)
        {
            Debug.LogWarning($"[EnemyCombat] {context.Owner.name}: controller or ground sensor is missing; cannot start combat action task.", context.Owner);
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
