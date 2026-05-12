using UnityEngine;
[CreateAssetMenu(fileName ="PlayerBaseConfig",menuName ="Config/Player")]
public  class PlayerBaseConfig : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float runVelocity;
    [SerializeField] private float walkVelocity;
    [SerializeField] private Vector2 jumpForce;
    [Header("Physics")]
    [SerializeField] private float gravityScale;


    public float RunVelocity => runVelocity;
    public float WalkVelocity => walkVelocity;
    public Vector2 JumpForce => jumpForce;
    public float GravityScale => gravityScale;
}
