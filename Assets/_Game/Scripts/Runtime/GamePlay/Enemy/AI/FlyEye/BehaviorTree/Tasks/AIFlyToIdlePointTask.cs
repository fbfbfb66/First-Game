using System;
using UnityEngine;

public sealed class AIFlyToIdlePointTask : AITask
{
    private readonly EnemyFlightMotor motor;
    private readonly Vector2 destination;
    private readonly float speed;
    private readonly float stoppingDistance;

    public override bool CanBeReplanned => Status == AITaskStatus.Running;

    public AIFlyToIdlePointTask(EnemyFlightMotor motor, Vector2 destination, float speed, float stoppingDistance)
    {
        if (motor == null) throw new ArgumentNullException(nameof(motor));
        if (!float.IsFinite(destination.x) || !float.IsFinite(destination.y))
            throw new ArgumentOutOfRangeException(nameof(destination));
        if (!float.IsFinite(speed) || speed <= 0f) throw new ArgumentOutOfRangeException(nameof(speed));
        if (!float.IsFinite(stoppingDistance) || stoppingDistance < 0f)
            throw new ArgumentOutOfRangeException(nameof(stoppingDistance));
        this.motor = motor;
        this.destination = destination;
        this.speed = speed;
        this.stoppingDistance = stoppingDistance;
    }

    protected override AITaskStatus OnTick(float deltaTime)
    {
        if (motor == null || !motor.isActiveAndEnabled) return AITaskStatus.Failed;
        float distance = Vector2.Distance(motor.Position, destination);
        if (distance <= stoppingDistance || Mathf.Approximately(distance, stoppingDistance))
            return AITaskStatus.Succeeded;
        motor.MoveTowards(destination, speed, stoppingDistance, deltaTime);
        return AITaskStatus.Running;
    }
}
