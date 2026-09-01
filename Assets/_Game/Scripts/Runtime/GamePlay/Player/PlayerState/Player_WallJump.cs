using UnityEngine;

public class Player_WallJump : Player_JumpUp
{
    public Player_WallJump(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        movement.HandleWallJump();
    }

    public override void PhysicalUpdate()
    {
    }
}
