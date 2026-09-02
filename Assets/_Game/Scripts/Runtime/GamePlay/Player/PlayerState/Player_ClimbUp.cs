using UnityEngine;

public class Player_ClimbUp : PlayerState
{
    private float originalGravityScale;
    public Player_ClimbUp(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        movement.ClearPlayerVelocity();
        animationTrigger.StartAnimation();
        originalGravityScale = movement.Rb.gravityScale;
        movement.Rb.gravityScale = 0f;
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (animationTrigger.IsAnimationFinished)
        {
            CommitAnimationPosition();
            ChangeStateToMoveState();
        }
    }

    public override void Exit()
    {
        base.Exit();
        movement.Rb.gravityScale = originalGravityScale;
    }

    private void CommitAnimationPosition()
    {
        Vector2 targetBodyPosition = anim.transform.position;
        movement.Teleport(targetBodyPosition);
        anim.transform.localPosition = Vector3.zero;
    }
}
