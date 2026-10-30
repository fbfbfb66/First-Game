using UnityEngine;

public class Player_DoubleJump : PlayerAir
{
    private MotionActionDefinition enteredAction;
    protected override MotionActionDefinition MotionAction => enteredAction;
    public Player_DoubleJump(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        player.MarkDoubleJumpUsed();
        enteredAction = player.DoubleJumpAction;
        stateName = enteredAction.Stage.AnimatorStateHash;
        base.Enter();
        movement.HandleDoubleJump(commandInput.MoveInput);
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
