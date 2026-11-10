using UnityEngine;

[CreateAssetMenu(fileName = "Task_FlyToIdlePoint", menuName = "Game/AI/Enemies/FlyEye/Tasks/Fly To Idle Point")]
public sealed class AIFlyToIdlePointTaskDefinition : AITaskDefinition
{
    [SerializeField, Min(0.01f)] private float speed = 5f;
    [SerializeField, Min(0f)] private float stoppingDistance = 0.1f;
    public float Speed => speed;
    public float StoppingDistance => stoppingDistance;

    public override AITask CreateTask(in AIAgentContext context)
    {
        if (context.FlightMotor == null || !context.IdlePosition.HasValue ||
            !float.IsFinite(speed) || speed <= 0f || !float.IsFinite(stoppingDistance) || stoppingDistance < 0f ||
            !float.IsFinite(context.IdlePosition.Value.x) || !float.IsFinite(context.IdlePosition.Value.y))
        {
            Debug.LogWarning($"[AIIdle] {name}: motor, idle position or movement settings are invalid; return task not created.", this);
            return null;
        }
        return new AIFlyToIdlePointTask(context.FlightMotor, context.IdlePosition.Value, speed, stoppingDistance);
    }
}
