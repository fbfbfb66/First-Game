using UnityEngine;

public class Player_Apex : PlayerAir
{
    public Player_Apex(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        float y = movement.GetCurrentVelocity().y;
        if(!groundSensor.IsGrounded && player.playerAnimationTrigger.IsAnimationFinished && y <= -player.playerBaseConfig.ApexThreshold)
        {
            stateMachine.ChangeState(player.fallState);
        }
    }
}
