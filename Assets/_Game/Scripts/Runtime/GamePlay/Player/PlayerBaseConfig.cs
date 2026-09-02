using UnityEngine;
[CreateAssetMenu(fileName ="PlayerBaseConfig",menuName ="Config/Player")]
public  class PlayerBaseConfig : ScriptableObject
{
    [Header("Move")]
    [SerializeField] private float runVelocity;
    [SerializeField] private float walkVelocity;
    [Header("Jump")]
    [SerializeField] private float jumpBufferDuration = 0.12f;
    [SerializeField] private float coyoteTimeDuration = 0.1f;
    [SerializeField] private Vector2 jumpForce;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField] private Vector2 doubleJumpForce;
    [SerializeField] private float fallGravity;
    [SerializeField] private float apexThreshold = 0.5f;
    [SerializeField] private float fallEnterVelocityThreshold = 5f;
    [SerializeField] private float rollingLandThresholdVelocity = -35f;
    [SerializeField] private float rollingLandDefaultVelocity = 5f;

    [Header("Run Conditions")]
    [SerializeField] private float canEndRunEarlyDuration = 3f;
    [SerializeField] private float runBufferDuration = 0.05f;
    [Header("Physics")]
    [SerializeField] private float gravityScale;
    [SerializeField] private float wallSlideSlowSpeed = -1f;
    [SerializeField] private float wallSlideFastSpeed = -3f;
    [SerializeField] private float coastingDuration = 0.2f;

    public float RollingLandDefaultVelocity => rollingLandDefaultVelocity;
    public float RollingLandThresholdVelocity => rollingLandThresholdVelocity;
    public Vector2 DoubleJumpForce => doubleJumpForce;
    public float JumpBufferDuration => jumpBufferDuration;
    public float CoyoteTimeDuration => coyoteTimeDuration;
    public float FallGravity => fallGravity;
    public float FallEnterVelocityThreshold => fallEnterVelocityThreshold;
    public float WallSlideFastSpeed => wallSlideFastSpeed;
    public float WallSlideSlowSpeed => wallSlideSlowSpeed;
    public float RunVelocity => runVelocity;
    public float WalkVelocity => walkVelocity;
    public Vector2 JumpForce => jumpForce;
    public Vector2 WallJumpForce => wallJumpForce;
    public float GravityScale => gravityScale;
    public float CoastingDuration => coastingDuration;
    public float CanEndRunEarlyDuration => canEndRunEarlyDuration;
    public float RunBufferDuration => runBufferDuration;
    public float ApexThreshold => apexThreshold;
}
