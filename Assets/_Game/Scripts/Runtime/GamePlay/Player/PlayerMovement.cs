using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform visualLayer;
    [Space]
    [SerializeField] private Player player;
    public bool facingRight {get;private set;} = true;
    private float runVelocity;
    private float walkVelocity;
    private Vector2 jumpForce;
    private Rigidbody2D playerRb;
    public PlayerMoveType playerMoveType{get;private set;}
    private void Awake()
    {
        if(player == null)
            player = GetComponentInParent<Player>();
    }
    private void Start()
    {
        SetPlayerMoveMode(PlayerMoveType.Run);
        playerRb = player.playerRb;
        if(playerRb != null)
        {
            InitializePlayerMovement(player.playerBaseConfig);
        }
    }

    public void HandleJump()
    {
        playerRb.linearVelocity += jumpForce;
    }

    public void HandleMove(Vector2 inputMove)
    {
        float moveVelocity = playerMoveType == PlayerMoveType.Run ? runVelocity : walkVelocity;
        Vector2 finalVelocity = new Vector2(moveVelocity * inputMove.x,playerRb.linearVelocity.y);
        SetRigibodyVelocity(finalVelocity);
        HandleFlip(inputMove);
    }

    public void HandleFlip(Vector2 inputMove)
    {
        if(inputMove.x < 0 && facingRight) Flip();
        if(inputMove.x > 0 && !facingRight) Flip();
    }

    public void SetRigibodyVelocity(Vector2 velocity)
    {
        playerRb.linearVelocity = velocity;
    }

    public void SetPlayerMoveMode(PlayerMoveType playerMoveType)
    {
        this.playerMoveType =playerMoveType;
    }
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = visualLayer.localScale;
        scale.x = -scale.x;
        visualLayer.localScale = scale;
    }
    private void InitializePlayerMovement(PlayerBaseConfig config)
    {
        if (config == null)
            return;

        walkVelocity = config.WalkVelocity;
        runVelocity = config.RunVelocity;
        jumpForce = config.JumpForce;
        playerRb.gravityScale = config.GravityScale;
    }
}
        

