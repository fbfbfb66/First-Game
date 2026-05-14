using UnityEngine;

public class Player_RunEndState : Player_RunTransition
{
    private float coastingVelocity;
    private float coastingTimer;
    public Player_RunEndState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }
    public override void Enter()
    {
        base.Enter();
        coastingVelocity = movement.GetMoveVelocity();
        coastingTimer = 0f;
    }
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (input.MoveInput.x == 0)
        {
            HandleCoasting();
        }
        if(movement.GetCurrentVelocity().x == 0)
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if (input.MoveInput.x != 0 && IsSameDirection())
        {
            stateMachine.ChangeState(player.runState);
        }
        else if (input.MoveInput.x != 0 && !IsSameDirection())
        {
            stateMachine.ChangeState(player.runTurnState);
        }
    }

    private void HandleCoasting()
    {
        if (coastingTimer > player.playerBaseConfig.CoastingDuration) return;
        coastingTimer += Time.deltaTime;
        float coastingProgress = coastingTimer / player.playerBaseConfig.CoastingDuration;
        float currentVelocity = Mathf.Lerp(coastingVelocity, 0, coastingProgress);
        currentVelocity = movement.facingRight ? currentVelocity : -currentVelocity;
        movement.SetRigibodyVelocity(new Vector2(currentVelocity, movement.GetCurrentVelocity().y));
    }
}

