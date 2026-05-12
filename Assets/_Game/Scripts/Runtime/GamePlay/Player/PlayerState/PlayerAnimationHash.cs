using UnityEngine;

public static class PlayerAnimationHash 
{
    public static readonly int Idle = Animator.StringToHash("Idle");
    public static readonly int Run = Animator.StringToHash("Run");
    public static readonly int Walk = Animator.StringToHash("Walk");
    public static readonly int Jump = Animator.StringToHash("Jump");
}
