using UnityEngine;

public class Player_RunTransition : PlayerState
{
    public Player_RunTransition(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
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
    }
    protected bool IsSameDirection()
    {
        if (input.MoveInput.x > 0 && movement.facingRight)
        {
            return true;
        }
        else if (input.MoveInput.x < 0 && !movement.facingRight)
        {
            return true;
        }
        return false;
    }
}
