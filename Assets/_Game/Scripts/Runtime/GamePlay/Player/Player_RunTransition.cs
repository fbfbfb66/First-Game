using UnityEngine;

public class Player_RunTransition : PlayerState
{
    public Player_RunTransition(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    protected bool IsSameDirection()
    {
        if (input.MoveInput.x > 0 && movement.facingRight)
        {
            return true;
        }
        else if (input.MoveInput.x < 0 && !movement.facingRight)
        {
            return true;
        }
        return false;
    }
}
