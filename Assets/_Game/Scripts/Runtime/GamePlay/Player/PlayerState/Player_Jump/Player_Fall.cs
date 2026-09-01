using UnityEngine;

public class Player_Fall : PlayerAir
{
    public Player_Fall(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(wallSensor.IsTouchingWall)
            stateMachine.ChangeState(player.wallSlideState);
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        movement.HandleMoveAndFlip(input.MoveInput);
    }
}
