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
        if(groundSensor.IsGrounded == false &&
           movement.GetCurrentVelocity().y <= -player.playerBaseConfig.FallEnterVelocityThreshold)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (input.ConsumeJump(player.playerBaseConfig.JumpBufferDuration))
        {
            stateMachine.ChangeState(player.jumpStartState);
            return;
        }

        if (input.ConsumeWorldInteract())
        {
            player.interaction.TryInteract();
        }
        if(groundSensor.IsGrounded)
            ChangeStateToMoveState();
    }
}
