using UnityEngine;

public class Player_RunState : PlayerMoveState
{
    public Player_RunState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    public override void Enter()
    {
        base.Enter();
        movement.SetPlayerMoveMode(PlayerMoveType.Run);
    }
}
