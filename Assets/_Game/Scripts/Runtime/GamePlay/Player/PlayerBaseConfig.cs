using UnityEngine;
[CreateAssetMenu(fileName ="PlayerBaseConfig",menuName ="Config/Player")]
public  class PlayerBaseConfig : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float runVelocity;
    [SerializeField] private float walkVelocity;
    [SerializeField] private Vector2 jumpForce;
    [Header("Run Conditions")]
    [SerializeField] private float canEndRunEarlyDuration = 3f;
    [SerializeField] private float runBufferDuration = 0.5f;
    [Header("Physics")]
    [SerializeField] private float gravityScale;
    [SerializeField] private float coastingDuration = 0.2f;


    public float RunVelocity => runVelocity;
    public float WalkVelocity => walkVelocity;
    public Vector2 JumpForce => jumpForce;
    public float GravityScale => gravityScale;
    public float CoastingDuration => coastingDuration;
    public float CanEndRunEarlyDuration => canEndRunEarlyDuration;
    public float RunBufferDuration => runBufferDuration;
}
