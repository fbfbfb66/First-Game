using UnityEngine;

public class Player_DoubleJump : PlayerAir
{
    public Player_DoubleJump(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        animationTrigger.StartAnimation();

        if(input.MoveInput.x == 0)
        {
            anim.CrossFade(PlayerAnimationHash.DoubleVerticalJump, 0);
        }
        else
        {
            anim.CrossFade(PlayerAnimationHash.DoubleForwardJump, 0);
            movement.HandleFlip(input.MoveInput);
        }
        movement.HandleDoubleJump(input.MoveInput);
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;

        if(animationTrigger.IsAnimationFinished)
        {
            stateMachine.ChangeState(player.fallState);
        }
    }
}
