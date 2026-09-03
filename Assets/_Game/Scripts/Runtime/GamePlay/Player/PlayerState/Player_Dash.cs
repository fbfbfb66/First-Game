using UnityEngine;

public class Player_Dash : PlayerState
{
    private float originalGravity;
    public Player_Dash(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        animationTrigger.StartAnimation();
        originalGravity = movement.Rb.gravityScale;
        movement.Rb.gravityScale = 0;
        
        float speedX = movement.facingRight ? player.playerBaseConfig.DefaultDashSpeed : -player.playerBaseConfig.DefaultDashSpeed;
        if(input.MoveInput.x != 0)
        {
            speedX = movement.facingRight ? player.playerBaseConfig.DashSpeed + movement.Rb.linearVelocity.x : -player.playerBaseConfig.DashSpeed + movement.Rb.linearVelocity.x;
        }

        if (isSameDirctionForWallandFacingDir())
        {
            Vector2 dir = movement.facingRight ? Vector2.left : Vector2.right;
            speedX = dir.x * player.playerBaseConfig.WallDashSpeed;
            movement.HandleFlip(dir);
        } 
        movement.SetRigibodyVelocity(new Vector2(speedX, 0));
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();

        if(groundSensor.IsGrounded == false && input.ConsumeJump(player.playerBaseConfig.JumpBufferDuration) && player.TryConsumeDoubleJump())
        {
            stateMachine.ChangeState(player.doubleJumpState);
            return;
        }


        if (animationTrigger.IsAnimationFinished)
        {
            if (groundSensor.CanEnterGrounded)
            {
                ChangeStateToMoveState();
                player.ResetDoubleJump();
                player.RequestDash();
                return;
            }

            if (isSameDirctionForWallandFacingDir())
            {
                stateMachine.ChangeState(player.wallSlideState);
                return;
            }

            if (groundSensor.IsGrounded == false)
            {
                stateMachine.ChangeState(player.fallState);
                return;
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        movement.Rb.gravityScale = originalGravity;
    }
}
