using UnityEngine;

public class Player_BackDodge : PlayerState
{
    private float originalGravity;

    protected override MotionActionDefinition MotionAction => player.BackDodgeAction;

    public Player_BackDodge(Player player, StateMachine stateMachine, int stateName, Animator anim)
        : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        originalGravity = movement.Rb.gravityScale;
        base.Enter();
        movement.Rb.gravityScale = player.playerBaseConfig.BackDodgeGravityScale;
        Vector2 velocity = player.playerBaseConfig.BackDodgeVelocity;
        velocity.x = movement.facingRight ? -Mathf.Abs(velocity.x) : Mathf.Abs(velocity.x);
        movement.SetRigibodyVelocity(velocity);
    }

    public override void Exit()
    {
        movement.Rb.gravityScale = originalGravity;
        base.Exit();
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;

        if (player.MotionController.BufferedRequest.Command == player.DashCommand)
        {
            ActionContext context = CreateActionContext();
            if (motionController.TryStartAction(player.DashAction, in context))
            {
                stateMachine.ChangeState(player.dashState);
                return;
            }
        }

        bool touchingGround = movement.Rb.IsTouchingLayers(groundSensor.GroundLayers);
        if (animationTrigger.IsAnimationFinished && touchingGround && groundSensor.CanEnterGrounded)
        {
            player.RestoreAirborneResources();
            ChangeStateToMoveState();
        }
    }

}
