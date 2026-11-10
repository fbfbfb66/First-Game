using UnityEngine;

public class EnemyFlightMotor : AIMotor
{
    public override void MoveTowards(Vector2 target,float speed,float stoppingDistance,float deltaTime)
    {
        if (Body == null || !isActiveAndEnabled || deltaTime <= 0 || BlocksLocomotion || DisplacementMotor.IsDisplacementActive) return;
        float distance = Vector2.Distance(Position, target);
        Vector2 dir = (target - Position).normalized;
        if (distance <= stoppingDistance)
        {
            return;
        }
        HandleFlip(dir);
        float remainingDistance = distance - stoppingDistance;
        float step = Mathf.Min(speed * deltaTime, remainingDistance);
        Vector2 nextPos = step * dir + Position;
        DisplacementMotor.RequestLocomotion(nextPos);
    }
}
