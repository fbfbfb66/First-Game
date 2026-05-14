using UnityEngine;

public class PlayerWalkState : PlayerGround
{
    public PlayerWalkState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
}
