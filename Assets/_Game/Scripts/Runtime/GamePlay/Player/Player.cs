using UnityEngine;

public class Player : Entity
{
    [Header("Data")]
    public PlayerBaseConfig playerBaseConfig;
    [Space]
    public PlayerMovement playerMovement;
    public PlayerInputReceiver playerInputReceiver;
    public Rigidbody2D playerRb;

    #region 
    public Player_IdleState idleState;
    public Player_RunState runState;
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

        
        idleState = new Player_IdleState(this,stateMachine,PlayerAnimationHash.Idle,anim);
        runState = new Player_RunState(this,stateMachine,PlayerAnimationHash.Run,anim);
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
