using UnityEngine;

public class PlayerAir : PlayerState
{
    public PlayerAir(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (groundSensor.IsGrounded)
        {
            ChangeStateToMoveState();
        }
    }

}
