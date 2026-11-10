using UnityEngine;

public readonly struct AIAgentContext
{
    public readonly Transform Owner;
    public readonly Transform Target;
    public readonly EnemyFlightMotor FlightMotor;
    public readonly EnemyCombatExecutor CombatExecutor;
    public readonly ActionDefinition SelectedAction;
    public readonly Vector2? IdlePosition;

    public AIAgentContext(Transform owner, Transform target, EnemyFlightMotor flightMotor, EnemyCombatExecutor combatExecutor, ActionDefinition selectedAction = null, Vector2? idlePosition = null)
    {
        Owner = owner;
        Target = target;
        FlightMotor = flightMotor;
        CombatExecutor = combatExecutor;
        SelectedAction = selectedAction;
        IdlePosition = idlePosition;
    }
}
