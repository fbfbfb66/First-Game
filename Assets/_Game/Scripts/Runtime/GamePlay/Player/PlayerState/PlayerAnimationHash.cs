using UnityEngine;

public static class PlayerAnimationHash 
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    

    public static readonly int Run = Animator.StringToHash("Run");
    public static readonly int RunTurn = Animator.StringToHash("RunTurn");
    public static readonly int RunEnd = Animator.StringToHash("RunEnd");


    public static readonly int Walk = Animator.StringToHash("Walk");

    public static readonly int JumpStart = Animator.StringToHash("JumpStart");
    public static readonly int JumpUp = Animator.StringToHash("JumpUp");
    public static readonly int Apex = Animator.StringToHash("Apex");
    public static readonly int Fall = Animator.StringToHash("Fall");
    public static readonly int BaseLand = Animator.StringToHash("BaseLand");
    public static readonly int RollingLand = Animator.StringToHash("RollingLand");
}
