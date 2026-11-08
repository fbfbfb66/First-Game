using UnityEngine;

public readonly struct AIAgentContext
{
    public readonly Transform Owner;
    public readonly Transform Target;
    public readonly EnemyFlightMotor FlightMotor;
    public readonly EnemyCombatExecutor CombatExecutor;

    public AIAgentContext(Transform owner, Transform target, EnemyFlightMotor flightMotor, EnemyCombatExecutor combatExecutor)
    {
        Owner = owner;
        Target = target;
        FlightMotor = flightMotor;
        CombatExecutor = combatExecutor;
    }
}
