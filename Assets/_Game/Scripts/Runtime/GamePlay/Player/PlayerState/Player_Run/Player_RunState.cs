using UnityEngine;

public class Player_RunState : Player_RunTransition
{
    private bool isFirstTimeRelese;
    private const string RunBufferTimerId = "RunBuffer";
    private const string RunEndEarlyTimerId = "RunEndEarly";
    public Player_RunState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isFirstTimeRelese = true;
        player.timeTool.StartTimeCounter(player.playerBaseConfig.CanEndRunEarlyDuration, RunEndEarlyTimerId);
    }
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if (stateMachine.currentState != this) return;

        ActionContext context = CreateActionContext();
        if (player.MotionController.BufferedRequest.Command == player.DashCommand &&
            motionController.TryStartAction(player.DashAction, in context))
        {
            stateMachine.ChangeState(player.dashState);
            return;
        }

        if (isSameDirctionForWallandFacingDir())
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        if(movement.playerMoveType == PlayerMoveType.Walk)
        {
            stateMachine.ChangeState(player.walkState);
            return;
        }
        if(commandInput.MoveInput.x == 0 && isFirstTimeRelese)
        {
            isFirstTimeRelese = false;
            player.timeTool.StartTimeCounter(player.playerBaseConfig.RunBufferDuration, RunBufferTimerId);
            return;
        }

        if(commandInput.MoveInput.x == 0 && !player.timeTool.IsRunning(RunEndEarlyTimerId) && !player.timeTool.IsRunning(RunBufferTimerId))
        {
            stateMachine.ChangeState(player.runEndState);
        }
        else if(commandInput.MoveInput.x == 0 && player.timeTool.IsRunning(RunEndEarlyTimerId) && !player.timeTool.IsRunning(RunBufferTimerId))
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if(!IsSameDirection() && commandInput.MoveInput.x != 0 && !player.timeTool.IsRunning(RunEndEarlyTimerId))
        {
            stateMachine.ChangeState(player.runTurnState);
        }
        else if(!IsSameDirection() && commandInput.MoveInput.x != 0 && player.timeTool.IsRunning(RunEndEarlyTimerId))
        {
            movement.HandleFlip(commandInput.MoveInput);
            isFirstTimeRelese = true;
            player.timeTool.StartTimeCounter(player.playerBaseConfig.CanEndRunEarlyDuration, RunEndEarlyTimerId);
        }
    }
    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        if(!player.timeTool.IsRunning(RunBufferTimerId))
        {
            if (!IsSameDirection())
            {
                movement.HandleFlip(commandInput.MoveInput);
                isFirstTimeRelese = true;
                player.timeTool.StartTimeCounter(player.playerBaseConfig.CanEndRunEarlyDuration, RunEndEarlyTimerId);
            } 
            movement.HandleMove(commandInput.MoveInput);
        }
        else
        {
            Vector2 velocity = movement.facingRight ? Vector2.right : Vector2.left;
            movement.HandleMove(velocity);
        }
    }
}
