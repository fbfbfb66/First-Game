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

        if (TryCancelToDash()) return;

        bool touchingGround = movement.Rb.IsTouchingLayers(groundSensor.GroundLayers);
        if (animationTrigger.IsAnimationFinished && touchingGround && groundSensor.CanEnterGrounded)
        {
            player.RestoreAirborneResources();
            ChangeStateToMoveState();
        }
    }

}
