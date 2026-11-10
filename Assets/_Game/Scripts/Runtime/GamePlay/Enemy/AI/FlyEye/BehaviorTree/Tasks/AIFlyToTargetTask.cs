using System;
using UnityEngine;

public class AIFlyToTargetTask : AITask
{
    private AIAgentContext context;
    private float speed;
    private float stoppingDistance;
    private float giveUpDistance;

    override public bool CanBeReplanned => true;

    public AIFlyToTargetTask(in AIAgentContext context, float speed, float stoppingDistance, float giveUpDistance)
    {

        if (speed <= 0 || stoppingDistance < 0 || giveUpDistance <= stoppingDistance || context.Motor == null)
        {
            throw new ArgumentNullException("非法数值");
        }
        this.context = context;
        this.speed = speed;
        this.stoppingDistance = stoppingDistance;
        this.giveUpDistance = giveUpDistance;
    }

    protected override AITaskStatus OnTick(float deltaTime)
    {
        if (context.Target == null || context.Target == context.Owner ||
            !context.Target.gameObject.activeInHierarchy || context.Motor == null)
        {
            return AITaskStatus.Failed;
        }
        float distance = Vector2.Distance(context.Owner.position, context.Target.position);
        if (distance > giveUpDistance)
        {
            return AITaskStatus.Failed;
        }
        if (!context.Motor.IsFacingTarget(context.Target.position))
            context.Motor.HandleFlip(context.Target.position - context.Owner.position);
        if (distance <= stoppingDistance || Mathf.Approximately(distance, stoppingDistance))
        {
            return AITaskStatus.Succeeded;
        }
        context.Motor.MoveTowards(context.Target.position, speed, stoppingDistance, deltaTime);
        return AITaskStatus.Running;
    }
}
