// AI supplies the action and context to the inherited TryStartAction.
// A motion executor must register action entry/exit through BeginState / EndState.
// This controller does not require an enemy FSM and does not move the body itself.
public class EnemyMotionController : MotionController
{
}
