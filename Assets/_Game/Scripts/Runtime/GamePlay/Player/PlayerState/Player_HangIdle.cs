using UnityEngine;

public class Player_HangIdle : PlayerState
{
    private float originalGravityScale;
    public Player_HangIdle(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        input.ClearJumpRequest();
        movement.ClearPlayerVelocity();
        Vector2 currentPos = movement.Rb.position;
        currentPos.y = wallSensor.HangBodyTargetY;
        movement.Rb.position = currentPos;
        originalGravityScale = movement.Rb.gravityScale;
        movement.Rb.gravityScale = 0;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(isSameDirctionForWallandFacingDir(false) == false)
        {
            stateMachine.ChangeState(player.fallState);
            Vector2 dir = movement.facingRight ? Vector2.left : Vector2.right;
            movement.HandleFlip(dir);
            return;
        }

        if (input.ConsumeJump(player.playerBaseConfig.JumpBufferDuration))
        {
            stateMachine.ChangeState(player.climbUpState);
            return;
        }


        
    }

    public override void Exit()
    {
        base.Exit();
        movement.Rb.gravityScale = originalGravityScale;
    }
}
