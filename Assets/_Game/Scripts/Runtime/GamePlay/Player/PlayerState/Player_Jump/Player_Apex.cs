using UnityEngine;

public class Player_Apex : PlayerAir
{
    public Player_Apex(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;

        ActionContext context = CreateActionContext();
        if (player.MotionController.BufferedRequest.Command == player.JumpCommand &&
            motionController.TryStartAction(player.DoubleJumpAction, in context))
        {
            stateMachine.ChangeState(player.doubleJumpState);
            return;
        }

        float y = movement.GetCurrentVelocity().y;
        if(!groundSensor.IsGrounded && animationTrigger.IsAnimationFinished && y <= -player.playerBaseConfig.ApexThreshold)
        {
            stateMachine.ChangeState(player.fallState);
        }
    }
}
