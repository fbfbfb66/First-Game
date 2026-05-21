using UnityEngine;

public class Player : Entity
{
    [Header("Data")]
    public PlayerBaseConfig playerBaseConfig;
    [Space]
    public PlayerMovement playerMovement;
    public PlayerInputReceiver playerInputReceiver;
    public PlayerAnimationTrigger playerAnimationTrigger;
    public InteractionDetector interaction;
    public TimeTool timeTool;
    public GroundSensor groundSensor;

    #region 
    public Player_IdleState idleState {get;private set;}
    public Player_RunState runState {get;private set;}

    public Player_RunTurnState runTurnState {get;private set;}
    public Player_RunEndState runEndState {get;private set;}

    public Player_JumpStart jumpStartState {get;private set;}
    public Player_JumpUp jumpUpState {get;private set;}
    public Player_Apex apexState {get;private set;}
    public Player_Fall fallState {get;private set;}
    #endregion

    protected override void Awake()
    {
        base.Awake();
        if(playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if(playerInputReceiver == null)
            playerInputReceiver = GetComponent<PlayerInputReceiver>();
        if(playerAnimationTrigger == null)
            playerAnimationTrigger = GetComponentInChildren<PlayerAnimationTrigger>();
        if(interaction == null)
            interaction = GetComponentInChildren<InteractionDetector>();
        if(timeTool == null)
            timeTool = GetComponent<TimeTool>();
        if(groundSensor == null)
            groundSensor = GetComponentInChildren<GroundSensor>();

        
        idleState = new Player_IdleState(this,stateMachine,PlayerAnimationHash.Idle,anim);
        runState = new Player_RunState(this,stateMachine,PlayerAnimationHash.Run,anim);
        runTurnState = new Player_RunTurnState(this,stateMachine,PlayerAnimationHash.RunTurn,anim);
        runEndState = new Player_RunEndState(this,stateMachine,PlayerAnimationHash.RunEnd,anim);
        jumpStartState = new Player_JumpStart(this,stateMachine,PlayerAnimationHash.JumpStart,anim);
        jumpUpState = new Player_JumpUp(this,stateMachine,PlayerAnimationHash.JumpUp,anim);
        apexState = new Player_Apex(this,stateMachine,PlayerAnimationHash.Apex,anim);
        fallState = new Player_Fall(this,stateMachine,PlayerAnimationHash.Fall,anim);
    }

    private void Start()
    {
        stateMachine.InitializeState(idleState);
    }

    private void Update()
    {
        stateMachine.LogicalUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicalUpdate();
    }
}
