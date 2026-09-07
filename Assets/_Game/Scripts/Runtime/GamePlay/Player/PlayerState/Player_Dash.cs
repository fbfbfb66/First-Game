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
        
        float speedX = movement.facingRight ? player.playerBaseConfig.DefaultDashSpeed + movement.Rb.linearVelocity.x * player.playerBaseConfig.DashFactor : -player.playerBaseConfig.DefaultDashSpeed + movement.Rb.linearVelocity.x * player.playerBaseConfig.DashFactor;

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

        if (TryCancelToJump()) return;


        if (animationTrigger.IsAnimationFinished)
        {
            if (groundSensor.CanEnterGrounded)
            {
                ChangeStateToMoveState();
                player.ResetDoubleJump();
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
        if (groundSensor.CanEnterGrounded)
            player.RequestDash();
    }

    private bool TryCancelToJump()
    {
        if (input.HasBufferedJump(player.playerBaseConfig.JumpBufferDuration) == false) return false;
        PlayerState targetState = null;
        if (groundSensor.IsGrounded) targetState = player.jumpStartState;
        else targetState = player.doubleJumpState;
        if(targetState == null) return false;
        if (stateMachine.CanChangeState(targetState, StateTransitionKind.Cancel) == false) return false;
        input.ConsumeJump(player.playerBaseConfig.JumpBufferDuration);
        if(targetState == player.doubleJumpState)
        {
            if (player.TryConsumeDoubleJump() == false) return false;
        }
        stateMachine.TryChangeState(targetState, StateTransitionKind.Cancel);
        return true;
    }
}
