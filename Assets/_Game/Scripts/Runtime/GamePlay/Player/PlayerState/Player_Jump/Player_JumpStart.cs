using UnityEngine;

public class Player_JumpStart : PlayerState
{
    private bool hadJumped;
    public Player_JumpStart(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    public override void Enter()
    {
        base.Enter();
        hadJumped = false;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(!hadJumped && animationTrigger.CanPerformAction)
        {
            movement.HandleJump();
            hadJumped = true;
        }
        if (animationTrigger.IsAnimationFinished)
        {
            stateMachine.ChangeState(player.jumpUpState);
        }
    }
}
