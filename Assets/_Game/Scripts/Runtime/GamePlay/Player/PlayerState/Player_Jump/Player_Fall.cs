using UnityEngine;

public class Player_Fall : PlayerAir
{
    private float lastVelocityY;
    public Player_Fall(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        movement.Rb.gravityScale = player.playerBaseConfig.FallGravity;
        lastVelocityY = movement.Rb.linearVelocity.y;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;

        if(groundSensor.IsGrounded == false && lastVelocityY != movement.Rb.linearVelocity.y)
        {
            lastVelocityY = movement.Rb.linearVelocity.y;
        }

        if (groundSensor.WasGroundedWithin(player.playerBaseConfig.CoyoteTimeDuration) && input.ConsumeJump(player.playerBaseConfig.JumpBufferDuration))
        {
            stateMachine.ChangeState(player.jumpStartState);
            return;
        }

        if (input.ConsumeJump(player.playerBaseConfig.JumpBufferDuration) && player.TryConsumeDoubleJump())
        {
            stateMachine.ChangeState(player.doubleJumpState);
            return;
        }

        if (isSameDirctionForWallandFacingDir())
            stateMachine.ChangeState(player.wallSlideState);
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        movement.HandleMoveAndFlip(input.MoveInput);
    }

    public override void Exit()
    {
        base.Exit();
        movement.Rb.gravityScale = player.playerBaseConfig.GravityScale;
    }

    protected override bool TryHandleLanding()
    {
        if (groundSensor.CanEnterGrounded && player.playerBaseConfig.RollingLandThresholdVelocity >= lastVelocityY)
        {
            stateMachine.ChangeState(player.rollingLandState);
            return true;
        }
        if (groundSensor.CanEnterGrounded)
        {
            ChangeStateToMoveState();
            return true;
        }
        return false;
    }
}
