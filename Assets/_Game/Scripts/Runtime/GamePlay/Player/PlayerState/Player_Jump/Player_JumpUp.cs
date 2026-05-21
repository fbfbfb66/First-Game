using UnityEngine;

public class Player_JumpUp : PlayerAir
{
    public Player_JumpUp(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void LogicalUpdate()
    {
        float y = movement.GetCurrentVelocity().y;
        if(y <= player.playerBaseConfig.ApexThreshold && !groundSensor.IsGrounded)
        {
            stateMachine.ChangeState(player.apexState);
        }
    }
}
