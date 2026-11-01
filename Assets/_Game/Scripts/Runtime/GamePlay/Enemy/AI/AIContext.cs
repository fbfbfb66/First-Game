using UnityEngine;

public readonly struct AIContext
{
    public readonly Transform Owner;
    public readonly Transform CandidateTarget;
    public readonly EnemyFlightMotor FlightMotor;
    public readonly EnemyCombatExecutor combatExecutor;

    public AIContext(Transform owner, Transform candidateTarget, EnemyFlightMotor flightMotor, EnemyCombatExecutor combatExecutor)
    {
        Owner = owner;
        CandidateTarget = candidateTarget;
        FlightMotor = flightMotor;
        this.combatExecutor = combatExecutor;
    }
}
