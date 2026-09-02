using UnityEngine;

public class Player_WallSlide : PlayerAir
{
    private float originalGravity;

    public Player_WallSlide(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.ResetDoubleJump();
        originalGravity = movement.Rb.gravityScale;
        movement.Rb.gravityScale = 0;
        movement.SetRigibodyVelocity(new Vector2(0,player.playerBaseConfig.WallSlideSlowSpeed));
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;
        if (isSameDirctionForWallandFacingDir() == false)
        {
            stateMachine.ChangeState(player.fallState);
            movement.HandleFlip(input.MoveInput);
            return;
        }

        if (input.ConsumeJump(player.playerBaseConfig.JumpBufferDuration))
        {
            stateMachine.ChangeState(player.wallJumpState);
            return;
        }

        Vector2 move = player.playerInputReceiver.MoveInput;
        if(move.y < 0)
        {
            movement.SetRigibodyVelocity(new Vector2(0,player.playerBaseConfig.WallSlideFastSpeed));
        }
        else
        {
            movement.SetRigibodyVelocity(new Vector2(0,player.playerBaseConfig.WallSlideSlowSpeed));
        }
    }

    public override void Exit()
    {
        base.Exit();
        movement.Rb.gravityScale = originalGravity;
    }


}
