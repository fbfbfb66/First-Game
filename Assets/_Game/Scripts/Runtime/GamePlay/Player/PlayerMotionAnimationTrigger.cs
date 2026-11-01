using UnityEngine;

public class PlayerMotionAnimationTrigger : MonoBehaviour
{
    [SerializeField] private MotionController motionController;
    public bool IsAnimationFinished { get; private set; }
    public bool CanPerformAction { get; private set; }

    private void Awake()
    {
        if (motionController == null) motionController = GetComponentInParent<MotionController>();
    }

    public void ResetAnimation()
    {
        IsAnimationFinished = false;
        CanPerformAction = false;
    }

    public void StartAnimation(AnimationEvent animationEvent)
    {
        if (IsCurrentAnimation(animationEvent)) ResetAnimation();
    }

    public void EndAnimation(AnimationEvent animationEvent)
    {
        if (IsCurrentAnimation(animationEvent)) IsAnimationFinished = true;
    }

    public void EnableAction(AnimationEvent animationEvent)
    {
        if (IsCurrentAnimation(animationEvent)) CanPerformAction = true;
    }

    public void DisableAction(AnimationEvent animationEvent)
    {
        if (IsCurrentAnimation(animationEvent)) CanPerformAction = false;
    }

    public void OpenMotionCancelWindow(AnimationEvent animationEvent)
    {
        if (!IsCurrentAnimation(animationEvent)) return;
        motionController.NotifyCancelWindowOpened(animationEvent.intParameter, animationEvent.animatorStateInfo.shortNameHash);
    }

    public void CloseMotionCancelWindow(AnimationEvent animationEvent)
    {
        if (!IsCurrentAnimation(animationEvent)) return;
        motionController.NotifyCancelWindowClosed(animationEvent.intParameter, animationEvent.animatorStateInfo.shortNameHash);
    }

    private bool IsCurrentAnimation(AnimationEvent animationEvent)
    {
        return motionController != null && animationEvent != null &&
            motionController.IsCurrentStateAnimation(animationEvent.animatorStateInfo.shortNameHash);
    }
}
