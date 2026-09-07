using UnityEngine;

public class Player : Entity
{
    private bool canDoubleJump = true;
    private bool canDash = true;

    [Header("Data")]
    public PlayerBaseConfig playerBaseConfig;
    [Space]
    public PlayerMovement playerMovement;
    public PlayerInputReceiver playerInputReceiver;
    public PlayerAnimationTrigger playerAnimationTrigger;
    public InteractionDetector interaction;
    public TimeTool timeTool;
    public GroundSensor groundSensor;
    public WallSensor wallSensor;
    [SerializeField] private PlayerCombatActionController combatActionController;
    public PlayerCombatActionController CombatActionController => combatActionController;
    [SerializeField] private CombatActionCommandKey dashCommand;
    public CombatActionCommandKey DashCommand => dashCommand;

    #region 
    public Player_IdleState idleState { get; private set; }

    public Player_WalkState walkState { get; private set; }

    public Player_RunState runState { get; private set; }
    public Player_RunTurnState runTurnState { get; private set; }
    public Player_RunEndState runEndState { get; private set; }

    public Player_JumpStart jumpStartState { get; private set; }
    public Player_JumpUp jumpUpState { get; private set; }
    public Player_Apex apexState { get; private set; }
    public Player_Fall fallState { get; private set; }

    public Player_WallSlide wallSlideState { get; private set; }
    public Player_WallJump wallJumpState { get; private set; }

    public Player_HangIdle hangIdleState { get; private set; }
    public Player_ClimbUp climbUpState { get; private set; }

    public Player_DoubleJump doubleJumpState { get; private set; }

    public Player_RollingLand rollingLandState { get; private set; }
    public Player_Dash dashState { get; private set; }
    public Player_CombatActionState combatActionState { get; private set; }
    public bool CanDoubleJump => canDoubleJump;
    public bool CanDash => canDash;
    #endregion

    public void RequestDash()
    {
        canDash = true;
    }
    public void ResetDoubleJump()
    {
        canDoubleJump = true;
    }

    public bool TryConsumeDash()
    {
        if (!canDash)
        {
            return false;
        }
        canDash = false;
        return true;
    }

    public bool TryConsumeDoubleJump()
    {
        if (!canDoubleJump)
        {
            return false;
        }

        canDoubleJump = false;
        return true;
    }

    protected override void Awake()
    {
        base.Awake();
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (playerInputReceiver == null)
            playerInputReceiver = GetComponent<PlayerInputReceiver>();
        if (playerAnimationTrigger == null)
            playerAnimationTrigger = GetComponentInChildren<PlayerAnimationTrigger>();
        if (interaction == null)
            interaction = GetComponentInChildren<InteractionDetector>();
        if (timeTool == null)
            timeTool = GetComponent<TimeTool>();
        if (groundSensor == null)
            groundSensor = GetComponentInChildren<GroundSensor>();
        if (wallSensor == null)
            wallSensor = GetComponentInChildren<WallSensor>();
        if (combatActionController == null)
            combatActionController = GetComponent<PlayerCombatActionController>();


        idleState = new Player_IdleState(this, stateMachine, PlayerAnimationHash.Idle, anim);
        walkState = new Player_WalkState(this, stateMachine, PlayerAnimationHash.Walk, anim);
        runState = new Player_RunState(this, stateMachine, PlayerAnimationHash.Run, anim);
        runTurnState = new Player_RunTurnState(this, stateMachine, PlayerAnimationHash.RunTurn, anim);
        runEndState = new Player_RunEndState(this, stateMachine, PlayerAnimationHash.RunEnd, anim);
        jumpStartState = new Player_JumpStart(this, stateMachine, PlayerAnimationHash.JumpStart, anim);
        jumpUpState = new Player_JumpUp(this, stateMachine, PlayerAnimationHash.JumpUp, anim);
        apexState = new Player_Apex(this, stateMachine, PlayerAnimationHash.Apex, anim);
        fallState = new Player_Fall(this, stateMachine, PlayerAnimationHash.Fall, anim);
        wallSlideState = new Player_WallSlide(this, stateMachine, PlayerAnimationHash.wallSlide, anim);
        wallJumpState = new Player_WallJump(this, stateMachine, PlayerAnimationHash.JumpUp, anim);
        hangIdleState = new Player_HangIdle(this, stateMachine, PlayerAnimationHash.HangIdle, anim);
        climbUpState = new Player_ClimbUp(this, stateMachine, PlayerAnimationHash.ClimbUp, anim);
        rollingLandState = new Player_RollingLand(this, stateMachine, PlayerAnimationHash.RollingLand, anim);
        dashState = new Player_Dash(this, stateMachine, PlayerAnimationHash.Dash, anim);
        doubleJumpState = new Player_DoubleJump(this, stateMachine, 0, anim);
        combatActionState = new Player_CombatActionState(this, stateMachine, 0, anim);



        idleState.AddCancelTransition(combatActionState);
        walkState.AddCancelTransition(combatActionState);
        jumpUpState.AddCancelTransition(combatActionState);
        runState.AddCancelTransition(combatActionState);
        dashState.AddCancelTransition(combatActionState, 0);
        dashState.AddCancelTransition(jumpStartState, 0);
        dashState.AddCancelTransition(doubleJumpState, 0);
    }

    private void Start()
    {
        stateMachine.InitializeState(idleState);
    }

    private void Update()
    {
        stateMachine.LogicalUpdate();
        TryEnterCombatAction();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicalUpdate();
    }

    public CombatActionContext CreateCombatActionContext()
    {
        CombatActionContext context = new CombatActionContext(groundSensor.IsGrounded,combatActionController.ChainRuntime);
        return context;
    }

    private void TryEnterCombatAction()
    {
        if (stateMachine.currentState == combatActionState) return;
        CombatActionContext context = CreateCombatActionContext();
        if (combatActionController.TryGetEntryActionCandidate(context, out var action) == false) return;
        if (stateMachine.CanChangeState(combatActionState, StateTransitionKind.Cancel) == false) return;
        if (combatActionController.TryConsumeBufferedRequest(action.Command) == false) return;

        combatActionController.BeginAction(action);
        stateMachine.TryChangeState(combatActionState, StateTransitionKind.Cancel);
    }

    public void NotifyStateCancelWindowOpened(int windowId, int sourceStateHash)
    {
        if (stateMachine.currentState is PlayerState currentPlayerState)
        {
            if (currentPlayerState.AnimatorStateHash != sourceStateHash) return;
            currentPlayerState.OpenCancelWindow(windowId);
            return;
        }
        Debug.LogWarning($"{stateMachine.currentState} Not PlayerState");
    }
    public void NotifyStateCancelWindowClosed(int windowId, int sourceStateHash)
    {
        if (stateMachine.currentState is PlayerState currentPlayerState)
        {
            if (currentPlayerState.AnimatorStateHash != sourceStateHash) return;
            currentPlayerState.CloseCancelWindow(windowId);
            return;
        }
        Debug.LogWarning($"{stateMachine.currentState} Not PlayerState");
    }
}
