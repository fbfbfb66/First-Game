using UnityEngine;

public class Player_Dash : PlayerState
{
    private float originalGravity;
    protected override MotionActionDefinition MotionAction => player.DashAction;
    public Player_Dash(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.MarkDashUsed();
        originalGravity = movement.Rb.gravityScale;
        movement.Rb.gravityScale = 0;
        
        float speed = player.playerBaseConfig.DefaultDashSpeed + Mathf.Abs(movement.Rb.linearVelocity.x) * player.playerBaseConfig.DashFactor;
        float speedX = movement.facingRight ? speed : -speed;

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
                player.RestoreAirborneResources();
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

    private bool TryCancelToJump()
    {
        if (motionController.BufferedRequest.Command != player.JumpCommand) return false;
        if (groundSensor.IsGrounded)
        {
            if (!motionController.AllowsCancel(player.JumpCommand)) return false;
            if (!motionController.TryConsumeBufferedRequest(player.JumpCommand)) return false;
            stateMachine.ChangeState(player.jumpStartState);
        }
        else
        {
            ActionContext context = CreateActionContext();
            if (!motionController.TryStartAction(player.DoubleJumpAction, in context)) return false;
            stateMachine.ChangeState(player.doubleJumpState);
        }
        return true;
    }
}
