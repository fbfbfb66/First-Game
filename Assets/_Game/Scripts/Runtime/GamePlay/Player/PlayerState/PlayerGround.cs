using UnityEngine;

public class PlayerGround : PlayerState
{
    public PlayerGround(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(input.MoveInput.x == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if(input.MoveInput.x != 0)
        {
            if(movement.playerMoveType == PlayerMoveType.Run)
            {
                stateMachine.ChangeState(player.runState);
            }
        }
    }
}
