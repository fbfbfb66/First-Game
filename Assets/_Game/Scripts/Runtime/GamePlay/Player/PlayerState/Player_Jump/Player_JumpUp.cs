using UnityEngine;

public class Player_JumpUp : PlayerAir
{
    public Player_JumpUp(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;

        ActionContext context = CreateActionContext();
        if (player.MotionController.BufferedRequest.Command == player.JumpCommand &&
            motionController.TryStartAction(player.DoubleJumpAction, in context))
        {
            stateMachine.ChangeState(player.doubleJumpState);
            return;
        }

        float y = movement.GetCurrentVelocity().y;
        if(y <= player.playerBaseConfig.ApexThreshold && !groundSensor.IsGrounded)
        {
            stateMachine.ChangeState(player.apexState);
        }
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        movement.HandleMoveAndFlip(commandInput.MoveInput);
    }
}
