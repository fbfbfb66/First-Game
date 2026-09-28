using UnityEngine;

public class PlayerAir : PlayerState
{
    public PlayerAir(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        input.ClearJumpRequest();
        input.ClearDashRequset();
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        if (TryHandleDash()) return;

        if (TryHandleLanding())
        {
            player.RestoreAirborneResources();
            return;
        }

        if (wallSensor.ReachedLedgeThisFrame)
        {
            stateMachine.ChangeState(player.hangIdleState);
            return;
        }
    }

    protected virtual bool TryHandleLanding()
    {
        if (groundSensor.CanEnterGrounded)
        {
            ChangeStateToMoveState();
            return true;
        }
        return false;
    }

    protected virtual bool TryHandleDash()
    {
        if (input.ConsumeDash() && player.TryConsumeDash())
        {
            stateMachine.ChangeState(player.dashState);
            return true;
        }
        return false;
    }

}
