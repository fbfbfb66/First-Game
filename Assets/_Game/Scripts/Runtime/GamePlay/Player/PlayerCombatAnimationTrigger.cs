using UnityEngine;

public class PlayerCombatAnimationTrigger : MonoBehaviour
{
    [SerializeField] private Player player;
    private PlayerCombatActionController combatActionController => player != null ? player.CombatActionController : null;

    private void Awake()
    {
        if (player == null) player = GetComponentInParent<Player>();
    }

    public void CommitAutomaticCombatTransitionOpened(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyAutomaticTransitionOpend(animationEvent.animatorStateInfo.shortNameHash);
    }
    public void CommitAutomaticCombatTransitionClosed(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyAutomaticTransitionClosed(animationEvent.animatorStateInfo.shortNameHash);
    }

    public void OpenCombatCancelWindow(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyCancelWindowOpened(
            animationEvent.intParameter,
            animationEvent.animatorStateInfo.shortNameHash);
    }
    public void CloseCombatCancelWindow(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyCancelWindowClosed(
            animationEvent.intParameter,
            animationEvent.animatorStateInfo.shortNameHash);
    }

    public void OpenCombatHitWindow(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyHitWindowOpened(
            animationEvent.intParameter,
            animationEvent.animatorStateInfo.shortNameHash);
    }
    public void CloseCombatHitWindow(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyHitWindowClosed(animationEvent.intParameter, animationEvent.animatorStateInfo.shortNameHash);
    }

    public void CommitCombatTransition(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyTransitionCommitOpened(
            animationEvent.animatorStateInfo.shortNameHash);
    }

    public void OpenCombatTransitionWindow(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyTransitionWindowOpened(
            animationEvent.animatorStateInfo.shortNameHash);
    }
    public void CloseCombatTransitionWindow(AnimationEvent animationEvent)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyTransitionWindowClosed(
            animationEvent.animatorStateInfo.shortNameHash);
    }
    public void FinishCombatStage(AnimationEvent animationEvent)
    {
        if(combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyStageFinished(
            animationEvent.animatorStateInfo.shortNameHash);
    }

}
