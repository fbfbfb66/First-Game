using System.Collections;
using UnityEngine;

public class PlayerMovement : Movement
{
    [Space]
    [SerializeField] private Player player;
    private float runVelocity;
    private float walkVelocity;
    private Vector2 jumpForce;
    public PlayerMoveType playerMoveType{get;private set;}
    protected override void Awake()
    {
        base.Awake();
        if(player == null)
            player = GetComponentInParent<Player>();
    }
    private void Start()
    {
        SetPlayerMoveMode(PlayerMoveType.Run);
        if(rb != null)
        {
            InitializePlayerMovement(player.playerBaseConfig);
        }
    }

    public float GetMoveVelocity()
    {
        return playerMoveType == PlayerMoveType.Run ? runVelocity : walkVelocity;
    }

    public void HandleJump()
    {
        float jumpX = facingRight ? jumpForce.x : -jumpForce.x;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + jumpX, jumpForce.y);
    }
    public override void HandleMoveAndFlip(Vector2 inputMove)
    {
        float moveVelocity = GetMoveVelocity();
        Vector2 finalVelocity = new Vector2(moveVelocity * inputMove.x,rb.linearVelocity.y);
        SetRigibodyVelocity(finalVelocity);
        HandleFlip(inputMove);
    }
    public void HandleMove(Vector2 inputMove)
    {
        float moveVelocity = GetMoveVelocity();
        Vector2 finalVelocity = new Vector2(moveVelocity * inputMove.x,rb.linearVelocity.y);
        SetRigibodyVelocity(finalVelocity);
    }

    public void SetPlayerMoveMode(PlayerMoveType playerMoveType)
    {
        this.playerMoveType =playerMoveType;
    }
    private void InitializePlayerMovement(PlayerBaseConfig config)
    {
        if (config == null)
            return;

        walkVelocity = config.WalkVelocity;
        runVelocity = config.RunVelocity;
        jumpForce = config.JumpForce;
        rb.gravityScale = config.GravityScale;
    }
}
        

