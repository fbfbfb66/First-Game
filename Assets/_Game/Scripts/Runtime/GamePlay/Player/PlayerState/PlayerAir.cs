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
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (TryHandleLanding())
        {
            player.ResetDoubleJump();
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

}
