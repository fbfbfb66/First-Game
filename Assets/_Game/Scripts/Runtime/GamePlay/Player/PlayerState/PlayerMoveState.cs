using UnityEngine;

public class PlayerMoveState : PlayerGround
{
    public PlayerMoveState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        movement.HandleMove(input.MoveInput);
    }
}
