using UnityEngine;

public class PlayerGround : PlayerState
{
    public PlayerGround(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(groundSensor.IsGrounded == false && movement.GetCurrentVelocity().y < 0)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (input.ConsumeJump())
        {
            stateMachine.ChangeState(player.jumpStartState);
            return;
        }

        if (input.ConsumeWorldInteract())
        {
            player.interaction.TryInteract();
        }
        
        ChangeStateToMoveState();
    }
}
