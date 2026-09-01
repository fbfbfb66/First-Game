using UnityEngine;

public class PlayerState : EntityState
{
    protected Player player;
    protected PlayerMovement movement;
    protected PlayerInputReceiver input;
    protected GroundSensor groundSensor;
    protected WallSensor wallSensor;
    protected PlayerAnimationTrigger animationTrigger;
    public PlayerState(Player player,StateMachine stateMachine, int stateName, Animator anim) : base(stateMachine, stateName, anim)
    {
        this.player = player;
        movement = player.playerMovement;
        input = player.playerInputReceiver;
        animationTrigger = player.playerAnimationTrigger;
        groundSensor = player.groundSensor;
        wallSensor = player.wallSensor;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        groundSensor.UpdateGroundState();
        wallSensor.UpdateWallState(movement.facingRight);

    }

    protected void ChangeStateToMoveState()
    {
        if(input.MoveInput.x == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if(isSameDirctionForWallandFacingDir() == false)
        {
            if(movement.playerMoveType == PlayerMoveType.Run)
            {
                stateMachine.ChangeState(player.runState);
            }
            else if(movement.playerMoveType == PlayerMoveType.Walk)
            {
                stateMachine.ChangeState(player.walkState);
            }
        }
        else
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    protected bool isSameDirctionForWallandFacingDir()
    {
        if (wallSensor.IsTouchingWall == false) return false;
        Vector2 move = input.MoveInput;
        bool isSameDir = false;
        if (move.x > 0 && movement.facingRight)
        {
            isSameDir = true;
        }
        if (move.x < 0 && movement.facingRight == false)
        {
            isSameDir = true;
        }
        return isSameDir;
    }
}
