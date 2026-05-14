using UnityEngine;

public class Player_RunState : Player_RunTransition
{
    private bool isFirstTimeRelese;
    private string RunBufferCoroutineID = "RunBuffer";
    private string RunEndEarlyCoroutineID = "RunEndEarly";
    public Player_RunState(Player player, StateMachine stateMachine, int stateName, Animator anim) : base(player, stateMachine, stateName, anim)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isFirstTimeRelese = true;
        player.timeTool.StartTimeCounter(player.playerBaseConfig.CanEndRunEarlyDuration, RunEndEarlyCoroutineID);
    }
    public override void LogicalUpdate()
    {
        base.LogicalUpdate();
        if(input.MoveInput.x == 0 && isFirstTimeRelese)
        {
            isFirstTimeRelese = false;
            player.timeTool.StartTimeCounter(player.playerBaseConfig.RunBufferDuration, RunBufferCoroutineID);
        }
        if(input.MoveInput.x == 0 && !player.timeTool.timeCounterCoroutines.ContainsKey(RunEndEarlyCoroutineID))
        {
            stateMachine.ChangeState(player.runEndState);
        }
        else if(input.MoveInput.x == 0 && player.timeTool.timeCounterCoroutines.ContainsKey(RunEndEarlyCoroutineID) && !player.timeTool.timeCounterCoroutines.ContainsKey(RunBufferCoroutineID))
        {
            stateMachine.ChangeState(player.idleState);
        }
        else if(!IsSameDirection() && input.MoveInput.x != 0 && !player.timeTool.timeCounterCoroutines.ContainsKey(RunEndEarlyCoroutineID))
        {
            stateMachine.ChangeState(player.runTurnState);
        }
        else if(!IsSameDirection() && input.MoveInput.x != 0 && player.timeTool.timeCounterCoroutines.ContainsKey(RunEndEarlyCoroutineID))
        {
            movement.HandleFlip(input.MoveInput);
            isFirstTimeRelese = true;
            player.timeTool.StartTimeCounter(player.playerBaseConfig.CanEndRunEarlyDuration, RunEndEarlyCoroutineID);
        }
    }
    public override void PhysicalUpdate()
    {
        base.PhysicalUpdate();
        if(!player.timeTool.timeCounterCoroutines.ContainsKey(RunBufferCoroutineID))
        {
            if (!IsSameDirection())
            {
                movement.HandleFlip(input.MoveInput);
                isFirstTimeRelese = true;
                player.timeTool.StartTimeCounter(player.playerBaseConfig.CanEndRunEarlyDuration, RunEndEarlyCoroutineID);
            } 
            movement.HandleMove(input.MoveInput);
        }
        else
        {
            Vector2 velocity = movement.facingRight ? Vector2.right : Vector2.left;
            movement.HandleMove(velocity);
        }
    }
}
