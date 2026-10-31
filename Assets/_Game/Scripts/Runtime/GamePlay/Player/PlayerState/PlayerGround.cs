using UnityEngine;

public class PlayerGround : PlayerState
{
    public PlayerGround(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        movement.ClearYVelocity();
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;
        if (TryStartBackDodge()) return;

        ActionContext context = CreateActionContext();
        if (motionController.BufferedRequest.Command == player.DashCommand &&
            motionController.TryStartAction(player.DashAction, in context))
        {
            stateMachine.ChangeState(player.dashState);
            return;
        }

        if(groundSensor.IsGrounded == false &&
           movement.GetCurrentVelocity().y <= -player.playerBaseConfig.FallEnterVelocityThreshold)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (motionController.TryConsumeBufferedRequest(player.JumpCommand))
        {
            stateMachine.ChangeState(player.jumpStartState);
            return;
        }

        if (motionController.TryConsumeBufferedRequest(player.InteractCommand))
        {
            player.interaction.TryInteract();
        }
        if(groundSensor.IsGrounded)
            ChangeStateToMoveState();
    }
}
