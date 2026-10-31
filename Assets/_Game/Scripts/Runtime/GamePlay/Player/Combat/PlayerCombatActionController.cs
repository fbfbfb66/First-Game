using System;
using UnityEngine;

public class PlayerCombatActionController : ActionController
{
    private CombatActionDefinition currentAction;
    private CombatActionStage currentStage;
    private CombatActionDefinition pendingAction;
    private CombatActionStage pendingStage;
    public override ActionStage CurrentStage => currentStage;
    public CombatActionStage CurrentCombatStage => currentStage;
    protected override ActionDefinition ActiveAction => currentAction;
    public CombatActionDefinition CurrentAction => currentAction;
    private bool isCurrentStageFinished;


    [SerializeField] private CombatActionDefinition[] entryActions;
    private bool isTransitionWindowOpen;
    private bool isAutomaticTransitionWindowOpen;
    private bool isTransitionCommitRequested;
    private CombatHitWindowDefinition activeHitWindow;
    private readonly CombatActionChainRuntime chainRuntime = new();
    private readonly CombatActionUsageRuntime usageRuntime = new();

    public CombatActionUsageRuntime UsageRuntime => usageRuntime;
    public CombatActionChainRuntime ChainRuntime => chainRuntime;
    public bool IsHitWindowOpen => activeHitWindow != null;
    public int ActiveHitWindowId => activeHitWindow.WindowId;
    public bool IsTransitionWindowOpen => isTransitionWindowOpen;
    public event Action HitWindowEnded;

    public bool TryQueueAutomaticTransition(in CombatActionContext context)
    {
        if (isAutomaticTransitionWindowOpen == false) return false;
        if (CurrentAction == null)
        {
            isAutomaticTransitionWindowOpen = false;
            return false;
        }
        if (pendingAction != null) return false;
        CombatActionTransition[] transitions = CurrentAction.Transitions;
        if(transitions == null || transitions.Length <= 0) return false;
        foreach (var transition in transitions)
        {
            if(transition == null) continue;
            if (transition.Command !=null)
            {
                Debug.LogWarning("Command must be null");   
                return false;
            }
            if (transition.TargetAction == null)
            {
                Debug.LogWarning("Target action is null");
                return false;
            }
            if (transition.AreConditionsMet(context) == false) continue;
            if (transition.TargetAction.AreEntryConditionsMet(context) == false) continue;
            if (!TryPrepareStage(transition.TargetAction, in context)) continue;
            isTransitionCommitRequested = true;
            isAutomaticTransitionWindowOpen = false;
            return true;
        }

        return false;
    }

    public bool TryCommitTransition(out CombatActionDefinition targetAction)
    {
        targetAction = null;
        if (isTransitionCommitRequested == false) return false;
        isTransitionCommitRequested = false;
        if (pendingAction == null) return false;
        targetAction = pendingAction;
        return true;
    }

    private void ClearCurrentActionRuntime()
    {
        EndHitWindow();
        currentAction = null;
        currentStage = null;
        pendingAction = null;
        pendingStage = null;
        isCurrentStageFinished = false;
        isTransitionWindowOpen = false;
        isAutomaticTransitionWindowOpen = false;
        hasBufferedRequest = false;
        isTransitionCommitRequested = false;
        activeCancelWindow = null;
        bufferedRequest = default;
        chainRuntime.Clear();
    }

    public bool TryQueueTransition(in CombatActionContext context)
    {
        if (currentAction == null) return false;
        if (pendingAction != null) return false;
        if (TryGetValidBufferedRequest(out var request) == false) return false;
        if (isTransitionWindowOpen == false) return false;
        CombatActionTransition[] transitions = currentAction.Transitions;
        if(transitions==null || transitions.Length <= 0) return false;
        foreach (var transition in transitions)
        {
            if (transition == null) continue;
            if (transition.Command == null)
            {
                Debug.LogWarning("Command is Null");
                return false;
            }
            if (transition.TargetAction == null)
            {
                Debug.LogWarning("Target action is null");
                return false;
            }
            if (request.Command != transition.Command) continue;
            if (transition.AreConditionsMet(context) == false) continue;
            if (transition.TargetAction.AreEntryConditionsMet(context) == false) continue;
            if (!TryPrepareStage(transition.TargetAction, in context)) continue;
            hasBufferedRequest = false;
            return true;
        }

        return false;
    }

    public bool TryGetEntryActionCandidate(in CombatActionContext context, out CombatActionDefinition action)
    {
        action = null;
        if (currentAction != null) return false;
        if (TryGetValidBufferedRequest(out var request) == false) return false;

        if (TryFindEntryActionCandidate(in context, in request, out var entryAction) == false) return false;

        action = entryAction;
        return true;
    }

    private bool TryFindEntryActionCandidate(in CombatActionContext context, in ActionRequest request, out CombatActionDefinition entryAction)
    {
        entryAction = null;
        if (entryActions == null || entryActions.Length == 0)
        {
            Debug.LogWarning("EntryActions is Null");
            return false;
        }

        foreach (var entry in entryActions)
        {
            if (entry == null)
            {
                Debug.LogWarning("EntryActions is wrong");
                continue;
            }
            if (entry.Command == null)
            {
                Debug.LogWarning($"[CombatAction] Entry action {entry.name} has no command.", entry);
                continue;
            }
            if (entry.Command != request.Command) continue;
            if (entry.AreEntryConditionsMet(in context) == false) continue;
            if (!TryPrepareStage(entry, in context)) continue;
            entryAction = entry;
            return true;
        }
        return false;
    }

    private bool TryPrepareStage(CombatActionDefinition action, in CombatActionContext context)
    {
        if (!action.TrySelectStage(in context, out var stage)) return false;
        if (stage is not CombatActionStage combatStage || string.IsNullOrWhiteSpace(stage.AnimatorStateName))
        {
            Debug.LogWarning($"[CombatAction] {name}: selected stage in {action.name} has no valid combat animation; request not consumed.", this);
            return false;
        }
        pendingAction = action;
        pendingStage = combatStage;
        return true;
    }

    public void ClearActionUsage()
    {
        usageRuntime.Clear();
    }

    public void CancelCurrentAction()
    {
        if (currentAction == null) return;
        ClearCurrentActionRuntime();
    }

    public bool TryCompleteAction()
    {
        if (currentAction == null) return false;
        if (isCurrentStageFinished == false) return false;
        ClearCurrentActionRuntime();
        return true;
    }

    public void BeginAction(CombatActionDefinition action)
    {
        if (action == null)
        {
            Debug.LogWarning("action is null");
            return;
        }
        if (pendingAction != action || pendingStage == null)
        {
            Debug.LogWarning($"[CombatAction] {name}: {action.name} has no pending stage; action not started.", this);
            return;
        }
        EndHitWindow();
        if (currentAction == null)
        {
            chainRuntime.Clear();
        }
        usageRuntime.Record(action);
        currentAction = action;
        currentStage = pendingStage;
        pendingAction = null;
        pendingStage = null;
        isCurrentStageFinished = false;
        isTransitionWindowOpen = false;
        isAutomaticTransitionWindowOpen = false;
        isTransitionCommitRequested = false;
        activeCancelWindow = null;
    }

    public void NotifyHitConfirmed(GameObject targetOwner)
    {
        if (currentAction == null)
        {
            Debug.LogWarning("CurrentAction is null");
            return;
        }
        if (targetOwner == null)
        {
            Debug.LogWarning("TargetOwner is null");
            return;
        }
        chainRuntime.RecordHit(currentAction, targetOwner);
    }

    public void NotifyHitWindowOpened(int windowId, int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        if (windowId < 0)
        {
            Debug.LogWarning("WindowId less 0");
            return;
        }
        if (IsHitWindowOpen)
        {
            Debug.LogWarning("HitWindow already Exit");
            return;
        }
        if (CurrentCombatStage.TryGetHitWindow(windowId, out var hitWindow) == false)
        {
            Debug.LogWarning($"Action : {CurrentStage.AnimatorStateName} didn`t find hitWindow compared whih {windowId}");
            return;
        }

        activeHitWindow = hitWindow;
    }
    public void NotifyHitWindowClosed(int windowId, int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        if (IsHitWindowOpen == false)
        {
            Debug.LogWarning("HitWindow Not Exit");
            return;
        }
        if (windowId != ActiveHitWindowId)
        {
            Debug.LogWarning("WindowId != activeHitWindowId");
            return;
        }
        EndHitWindow();
    }

    private void EndHitWindow()
    {
        activeHitWindow = null;
        HitWindowEnded?.Invoke();
    }

    public void NotifyAutomaticTransitionOpend(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        isAutomaticTransitionWindowOpen = true;
    }
    public void NotifyAutomaticTransitionClosed(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        isAutomaticTransitionWindowOpen = false;
    }

    public void NotifyTransitionCommitOpened(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        isTransitionWindowOpen = false;
        isTransitionCommitRequested = true;
    }

    public void NotifyTransitionWindowClosed(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        if (isTransitionWindowOpen == false)
        {
            Debug.LogWarning("window has closed");
            return;
        }
        isTransitionWindowOpen = false;
    }

    public void NotifyTransitionWindowOpened(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        if (isTransitionWindowOpen)
        {
            Debug.LogWarning("window has opened");
            return;
        }
        isTransitionWindowOpen = true;
    }


    public void NotifyStageFinished(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        isCurrentStageFinished = true;
    }
}
