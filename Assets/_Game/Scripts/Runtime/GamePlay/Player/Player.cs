using UnityEngine;

public class Player : Entity
{
    [Header("Data")]
    public PlayerBaseConfig playerBaseConfig;
    [Space]
    public PlayerMovement playerMovement;
    public PlayerInputReceiver playerInputReceiver;
    public PlayerAnimationTrigger playerAnimationTrigger;
    public Rigidbody2D playerRb;
    public TimeTool timeTool;

    #region 
    public Player_IdleState idleState;
    public Player_RunState runState;

    public Player_RunTurnState runTurnState;
    public Player_RunEndState runEndState;
    
    #endregion

    protected override void Awake()
    {
        base.Awake();
        if(playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
        if(playerRb == null)
            playerRb = GetComponent<Rigidbody2D>();
        if(playerInputReceiver == null)
            playerInputReceiver = GetComponent<PlayerInputReceiver>();
        if(playerAnimationTrigger == null)
            playerAnimationTrigger = GetComponentInChildren<PlayerAnimationTrigger>();
        if(timeTool == null)
            timeTool = GetComponent<TimeTool>();

        
        idleState = new Player_IdleState(this,stateMachine,PlayerAnimationHash.Idle,anim);
        runState = new Player_RunState(this,stateMachine,PlayerAnimationHash.Run,anim);
        runTurnState = new Player_RunTurnState(this,stateMachine,PlayerAnimationHash.RunTurn,anim);
        runEndState = new Player_RunEndState(this,stateMachine,PlayerAnimationHash.RunEnd,anim);
    }

    private void Start()
    {
        stateMachine.InitializeState(idleState);
    }

    private void Update()
    {
        stateMachine.LogicialUpdate();
    }

    private void FixedUpdate()
    {
        stateMachine.PhysicalUpdate();
    }
}
