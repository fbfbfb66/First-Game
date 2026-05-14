using UnityEngine;

public class Player_RunTurnState : Player_RunTransition
{
    public Player_RunTurnState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(animationTrigger.IsAnimationFinished && input.MoveInput.x == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if (animationTrigger.IsAnimationFinished && input.MoveInput.x != 0)
        {
            stateMachine.ChangeState(player.runState);
        }
    }

    public override void Exit()
    {
        base.Exit();
        movement.HandleFlip(input.MoveInput);
    }
}
