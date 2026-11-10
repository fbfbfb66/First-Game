using UnityEngine;

public readonly struct AIAgentContext
{
    public readonly Transform Owner;
    public readonly Transform Target;
    public readonly AIMotor Motor;
    public readonly EnemyCombatExecutor CombatExecutor;
    public readonly ActionDefinition SelectedAction;
    public readonly Vector2? IdlePosition;
    public readonly ActiveArea Area;
    public readonly AITargetPerception Perception;

    public AIAgentContext(Transform owner, AITargetPerception perception, AIMotor motor,EnemyCombatExecutor combatExecutor,ActiveArea area
        ,ActionDefinition selectedAction = null, Vector2? idlePosition = null)
    {
        Owner = owner;
        Perception = perception;
        Target = perception != null && perception.DetectedTarget != null
            ? perception.DetectedTarget.transform : null;
        Motor = motor;
        CombatExecutor = combatExecutor;
        SelectedAction = selectedAction;
        IdlePosition = idlePosition;
        Area = area;
    }
}
