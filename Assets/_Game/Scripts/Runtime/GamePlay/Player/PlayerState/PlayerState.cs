using System.Collections.Generic;
using UnityEngine;

public class PlayerState : EntityState
{
    protected Player player;
    protected PlayerMovement movement;
    protected PlayerInputReceiver input;
    protected GroundSensor groundSensor;
    protected WallSensor wallSensor;
    protected PlayerAnimationTrigger animationTrigger;
    private int activeCancelWindowId = -1;
    private readonly List<StateCancelTransitionRule> cancelTransitions = new();

    public bool HasActiveCancelWindow => activeCancelWindowId >= 0;
    public int ActiveCancelWindowId => activeCancelWindowId;
    public PlayerState(Player player,StateMachine stateMachine, int stateName, Animator anim) : base(stateMachine, stateName, anim)
    {
        this.player = player;
        movement = player.playerMovement;
        input = player.playerInputReceiver;
        animationTrigger = player.playerAnimationTrigger;
        groundSensor = player.groundSensor;
        wallSensor = player.wallSensor;
    }

    public void AddCancelTransition(EntityState targetState,int requiredWindowId = -1)
    {
        StateCancelTransitionRule rule = new StateCancelTransitionRule(targetState, requiredWindowId);
        cancelTransitions.Add(rule);
    }

    public override bool CanTransitionTo(EntityState targetState, StateTransitionKind transitionKind)
    {
        if (transitionKind != StateTransitionKind.Cancel) return base.CanTransitionTo(targetState, transitionKind);
        foreach(var transitionRule in cancelTransitions)
        {
            if (transitionRule == null) continue;
            if (transitionRule.Allows(targetState, activeCancelWindowId)) return true;
        }
        return false;
    }

    public void OpenCancelWindow(int windowId)
    {
        if(windowId < 0)
        {
            Debug.LogWarning($"WindowId less 0");
            return;
        }
        if (HasActiveCancelWindow)
        {
            Debug.LogWarning($"CancelWindow is Exit");
            return;
        }
        activeCancelWindowId = windowId;
        Debug.Log($"[FSM Window] Open | Source={GetType().Name} | Window={windowId}");
    }

    public void CloseCancelWindow(int windowId)
    {
        if (windowId != activeCancelWindowId)
        {
            Debug.LogWarning("windowId Not Equal to activeCancelWindowId ");
            return;
        }
        if (HasActiveCancelWindow==false)
        {
            Debug.LogWarning($"CancelWindow Not Exit");
            return;
        }
        activeCancelWindowId = -1;
        Debug.Log($"[FSM Window] Close | Source={GetType().Name} | Window={windowId}");
    }

    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        groundSensor.UpdateGroundState();
        wallSensor.UpdateWallState(movement.facingRight);

    }

    protected void ChangeStateToMoveState()
    {
        if(input.MoveInput.x == 0)
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
        Vector2 move = input.MoveInput;
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
        base.Exit();
        if (HasActiveCancelWindow)
        {
            Debug.Log("FSM| CleanUp");
            activeCancelWindowId = -1;
        }
    }
}
