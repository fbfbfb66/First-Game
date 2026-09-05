using UnityEngine;

public class PlayerAnimationTrigger : MonoBehaviour
{
    [SerializeField] private PlayerCombatActionController combatActionController;
    public bool IsAnimationFinished { get; private set; }
    public bool canPerformAction { get; private set; }

    private void Awake()
    {
        if (combatActionController == null)
            combatActionController = GetComponentInParent<PlayerCombatActionController>();
    }

    public void OpenCombatHitWindow(int windowId)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyHitWindowOpened(windowId);
    }
    public void CloseCombatHitWindow(int windowId)
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyHitWindowClosed(windowId);
    }

    public void CommitCombatTransition()
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyTransitionCommitRequested();
    }

    public void OpenCombatTransitionWindow()
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyTransitionWindowOpened();
    }
    public void CloseCombatTransitionWindow()
    {
        if (combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyTransitionWindowClosed();
    }
    public void FinishCombatStage()
    {
        if(combatActionController == null)
        {
            Debug.LogWarning("Missed PlayerCombatActionController");
            return;
        }
        combatActionController.NotifyStageFinished();
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
