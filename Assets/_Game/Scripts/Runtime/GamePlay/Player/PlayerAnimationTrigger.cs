using UnityEngine;

public class PlayerAnimationTrigger : MonoBehaviour
{
    [SerializeField] private PlayerCombatActionController combatActionController;
    [SerializeField] private Player player;
    public bool IsAnimationFinished { get; private set; }
    public bool canPerformAction { get; private set; }

    private void Awake()
    {
        if (combatActionController == null)
            combatActionController = GetComponentInParent<PlayerCombatActionController>();
        if(player == null)
            player = GetComponentInParent<Player>();
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

    public void OpenStateCancelWindow(AnimationEvent animationEvent)
    {
        if(player == null)
        {
            Debug.LogWarning("Player Null");
            return;
        }
        player.NotifyStateCancelWindowOpened(
            animationEvent.intParameter,
            animationEvent.animatorStateInfo.shortNameHash);
    }
    public void CloseStateCancelWindow(AnimationEvent animationEvent)
    {
        if (player == null)
        {
            Debug.LogWarning("Player Null");
            return;
        }
        player.NotifyStateCancelWindowClosed(
            animationEvent.intParameter,
            animationEvent.animatorStateInfo.shortNameHash);
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

    public void EndAnimation()
    {
        IsAnimationFinished = true;
    }
    public void StartAnimation()
    {
        IsAnimationFinished = false;
    }

    public void EnableAction()
    {
        canPerformAction = true;
    }
    public void DisableAction()
    {
        canPerformAction = false;
    }
}
