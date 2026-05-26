using UnityEngine;

public class PlayerState : EntityState
{
    protected Player player;
    protected PlayerMovement movement;
    protected PlayerInputReceiver input;
    protected GroundSensor groundSensor;
    protected PlayerAnimationTrigger animationTrigger;
    public PlayerState(Player player,StateMachine stateMachine, int stateName, Animator anim) : base(stateMachine, stateName, anim)
    {
        this.player = player;
        movement = player.playerMovement;
        input = player.playerInputReceiver;
        animationTrigger = player.playerAnimationTrigger;
        groundSensor = player.groundSensor;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        groundSensor.UpdateGroundState();
    }

    protected void ChangeStateToMoveState()
    {
        if(input.MoveInput.x == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else
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
    }
}
