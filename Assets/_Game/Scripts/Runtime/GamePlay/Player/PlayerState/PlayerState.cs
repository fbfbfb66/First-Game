using UnityEngine;

public class PlayerState : EntityState
{
    protected Player player;
    protected PlayerMovement movement;
    protected PlayerCommandInput commandInput;
    protected PlayerMotionController motionController;
    protected GroundSensor groundSensor;
    protected WallSensor wallSensor;
    protected PlayerMotionAnimationTrigger animationTrigger;
    protected virtual MotionActionDefinition MotionAction => null;

    public PlayerState(Player player,StateMachine stateMachine, int stateName, Animator anim) : base(stateMachine, stateName, anim)
    {
        this.player = player;
        movement = player.playerMovement;
        commandInput = player.CommandInput;
        motionController = player.MotionController;
        animationTrigger = player.MotionAnimationTrigger;
        groundSensor = player.groundSensor;
        wallSensor = player.wallSensor;
    }

    public override void Enter()
    {
        motionController.BeginState(stateName, MotionAction);
        animationTrigger.ResetAnimation();
        base.Enter();
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        groundSensor.UpdateGroundState();
        wallSensor.UpdateWallState(movement.facingRight);

    }

    protected ActionContext CreateActionContext()
    {
        return new ActionContext(
            groundSensor.IsGrounded,
            groundSensor.CanEnterGrounded,
            groundSensor.transform.position,
            player.CanDash,
            player.CanDoubleJump,
            player.Skills);
    }

    protected void ChangeStateToMoveState()
    {
        if(commandInput.MoveInput.x == 0)
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

    protected bool isSameDirctionForWallandFacingDir(bool needToHandleKey = true)
    {
        if (wallSensor.IsTouchingWall == false) return false;
        Vector2 move = commandInput.MoveInput;
        bool isSameDir = false;
        if (move.x > 0 && movement.facingRight)
        {
            isSameDir = true;
        }
        if (move.x < 0 && movement.facingRight == false)
        {
            isSameDir = true;
        }
        if (needToHandleKey == false && move.x == 0) return true;
        return isSameDir;
    }

    public override void Exit()
    {
        motionController.EndState(stateName);
        base.Exit();
    }
}
