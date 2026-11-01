using System;
using UnityEngine;

public class AIFlyToTargetTask : AITask
{
    private AIContext context;
    private float speed;
    private float stoppingDistance;
    private float giveUpDistance;

    public AIFlyToTargetTask(in AIContext context, float speed, float stoppingDistance, float giveUpDistance)
    {

        if (speed <= 0 || stoppingDistance < 0 || giveUpDistance <= stoppingDistance || context.FlightMotor == null)
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
        if (context.CandidateTarget == null || context.CandidateTarget == context.Owner || context.FlightMotor == null)
        {
            return AITaskStatus.Failed;
        }
        float distance = Vector2.Distance(context.Owner.position, context.CandidateTarget.position);
        if (distance > giveUpDistance)
        {
            return AITaskStatus.Failed;
        }
        else if (distance <= stoppingDistance || Mathf.Approximately(distance, stoppingDistance))
        {
            return AITaskStatus.Succeeded;
        }
        context.FlightMotor.MoveTowards(context.CandidateTarget.position, speed, stoppingDistance, deltaTime);
        return AITaskStatus.Running;
    }
}
