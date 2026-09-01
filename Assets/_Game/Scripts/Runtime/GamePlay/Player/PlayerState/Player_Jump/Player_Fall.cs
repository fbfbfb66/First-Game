using UnityEngine;

public class Player_Fall : PlayerAir
{
    public Player_Fall(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        movement.Rb.gravityScale = player.playerBaseConfig.FallGravity;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (isSameDirctionForWallandFacingDir())
            stateMachine.ChangeState(player.wallSlideState);
    }

    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        movement.HandleMoveAndFlip(input.MoveInput);
    }

    public override void Exit()
    {
        base.Exit();
        movement.Rb.gravityScale = player.playerBaseConfig.GravityScale;
    }
}
