using UnityEngine;

public class Player_RunTransition : PlayerState
{
    public Player_RunTransition(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
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

        if (motionController.TryConsumeBufferedRequest(player.JumpCommand))
        {
            stateMachine.ChangeState(player.jumpStartState);
            return;
        }
        if (motionController.TryConsumeBufferedRequest(player.InteractCommand))
        {
            player.interaction.TryInteract();
        }
    }
    protected bool IsSameDirection()
    {
        if (commandInput.MoveInput.x > 0 && movement.facingRight)
        {
            return true;
        }
        else if (commandInput.MoveInput.x < 0 && !movement.facingRight)
        {
            return true;
        }
        return false;
    }
}
