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
        if (stateMachine.currentState != this) return;
        if (TryStartBackDodge()) return;


        if(groundSensor.IsGrounded == false &&
           movement.GetCurrentVelocity().y <= -player.playerBaseConfig.FallEnterVelocityThreshold)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (player.MotionController.TryConsumeBufferedRequest(player.JumpCommand))
        {
            stateMachine.ChangeState(player.jumpStartState);
            return;
        }
        if (player.MotionController.TryConsumeBufferedRequest(player.InteractCommand))
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
