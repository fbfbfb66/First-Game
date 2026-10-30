using UnityEngine;

public class PlayerAir : PlayerState
{
    public PlayerAir(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        ActionContext context = CreateActionContext();
        if (motionController.BufferedRequest.Command == player.DashCommand &&
            motionController.TryStartAction(player.DashAction, in context))
        {
            stateMachine.ChangeState(player.dashState);
            return;
        }

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



}
