using UnityEngine;

public class Player_DoubleJump : PlayerAir
{
    protected override MotionActionDefinition MotionAction => player.DoubleJumpAction;
    public Player_DoubleJump(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        player.MarkDoubleJumpUsed();
        if (!Mathf.Approximately(commandInput.MoveInput.x, 0f))
            movement.HandleFlip(commandInput.MoveInput);
        base.Enter();
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
