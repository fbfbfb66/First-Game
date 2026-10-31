using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerHealth))]
public class Player : Entity
{
    private bool canDoubleJump = true;
    private bool canDash = true;
    
    [SerializeField] private PlayerSkills  playerSkills;
    public PlayerSkills Skills => playerSkills;

    [Header("Data")]
    public PlayerBaseConfig playerBaseConfig;
    [SerializeField] private CharacterStatsData characterStatsData;
    public CharacterStats Stats { get; private set; }
    public PlayerHealth Health { get; private set; }
    public PlayerCommandInput CommandInput { get; } = new();
    [Space]
    public PlayerMovement playerMovement;
    [SerializeField] private PlayerMotionAnimationTrigger motionAnimationTrigger;
    public PlayerMotionAnimationTrigger MotionAnimationTrigger => motionAnimationTrigger;
    public InteractionDetector interaction;
    public TimeTool timeTool;
    public GroundSensor groundSensor;
    public WallSensor wallSensor;
    [SerializeField] private PlayerMotionController motionController;
    [SerializeField] private PlayerCombatActionController combatActionController;
    public PlayerMotionController MotionController => motionController;
    public PlayerCombatActionController CombatActionController => combatActionController;
    [Space]
    [Header("Actions Settings")]
    [SerializeField] private MotionActionDefinition dashAction;
    [SerializeField] private MotionActionDefinition backDodgeAction;
    [SerializeField] private MotionActionDefinition wallSlideAction;
    [FormerlySerializedAs("doubleJumpVerticalAction")]
    [SerializeField] private MotionActionDefinition doubleJumpAction;

    [SerializeField] private ActionCommandKey dashCommand;
    [SerializeField] private ActionCommandKey jumpCommand;
    [SerializeField] private ActionCommandKey interactCommand;

    public MotionActionDefinition DashAction => dashAction;
    public MotionActionDefinition BackDodgeAction => backDodgeAction;
    public ActionCommandKey BackDodgeCommand => backDodgeAction != null ? backDodgeAction.Command : null;
    public MotionActionDefinition WallSlideAction => wallSlideAction;
    public MotionActionDefinition DoubleJumpAction => doubleJumpAction;

    public ActionCommandKey DashCommand => dashCommand;
    public ActionCommandKey JumpCommand => jumpCommand;
    public ActionCommandKey InteractCommand => interactCommand;

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
    public Player_BackDodge backDodgeState { get; private set; }
    public Player_CombatActionState combatActionState { get; private set; }
    public bool CanDoubleJump => canDoubleJump;
    public bool CanDash => canDash;
    #endregion

    public void ResetDash()
    {
        canDash = true;
    }
    public void ResetDoubleJump()
    {
        canDoubleJump = true;
    }

    public void MarkDashUsed()
    {
        canDash = false;
    }

    public void MarkDoubleJumpUsed()
    {
        canDoubleJump = false;
    }

    protected override void Awake()
    {
        base.Awake();
        Health = GetComponent<PlayerHealth>();
        if (characterStatsData == null)
            Debug.LogWarning($"{name} cannot initialize runtime stats: CharacterStatsData is not assigned. Stats remain unavailable.", this);
        else
        {
            Stats = new CharacterStats(characterStatsData);
            Health.Initialize(Stats);
        }

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if (motionAnimationTrigger == null)
            motionAnimationTrigger = GetComponentInChildren<PlayerMotionAnimationTrigger>();
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
        if(motionController == null)
            motionController = GetComponent<PlayerMotionController>();


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
        backDodgeState = new Player_BackDodge(this, stateMachine, 0, anim);
        doubleJumpState = new Player_DoubleJump(this, stateMachine, 0, anim);
        combatActionState = new Player_CombatActionState(this, stateMachine, 0, anim);



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

    private void OnDisable()
    {
        CommandInput.ClearMoveInput();
    }

    public CombatActionContext CreateCombatActionContext()
    {
        CombatActionContext context = new CombatActionContext(
            groundSensor.IsGrounded,
            groundSensor.CanEnterGrounded,
            groundSensor.transform.position,
            combatActionController.ChainRuntime,
            combatActionController.UsageRuntime,
            playerSkills);
        return context;
    }

    private void TryEnterCombatAction()
    {
        if (stateMachine.currentState == combatActionState) return;
        CombatActionContext context = CreateCombatActionContext();
        if (combatActionController.TryGetEntryActionCandidate(context, out var action) == false) return;
        if (!motionController.AllowsCancel(action.Command)) return;
        if (!stateMachine.CanChangeState(combatActionState, StateTransitionKind.Natural)) return;
        if (combatActionController.TryConsumeBufferedRequest(action.Command) == false) return;

        combatActionController.BeginAction(action);
        stateMachine.ChangeState(combatActionState);
    }

    public void RestoreAirborneResources()
    {
        ResetDoubleJump();
        ResetDash();
        combatActionController.ClearActionUsage();
    }

}
