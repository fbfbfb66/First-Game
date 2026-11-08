using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatActionController : ActionController
{
    public CombatActionController(UnityEngine.Object owner) : base(owner) { }

    private CombatActionDefinition currentAction;
    private CombatActionStage currentStage;
    private CombatActionDefinition pendingAction;
    private CombatActionStage pendingStage;
    public override ActionStage CurrentStage => currentStage;
    public CombatActionStage CurrentCombatStage => currentStage;
    protected override ActionDefinition ActiveAction => currentAction;
    public CombatActionDefinition CurrentAction => currentAction;
    private bool isCurrentStageFinished;


    private bool isAutomaticTransitionWindowOpen;
    private bool isTransitionCommitRequested;
    private CombatHitWindowDefinition activeHitWindow;
    private readonly CombatActionChainRuntime chainRuntime = new();
    private readonly CombatActionUsageRuntime usageRuntime = new();
    private readonly Dictionary<CombatActionDefinition, double> cooldownEndTimes = new();
    private double cooldownTime;

    public void TickCooldowns(float deltaTime)
    {
        if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
        {
            Debug.LogWarning($"[CombatAction] {OwnerName}: invalid cooldown delta time {deltaTime}; timer not advanced.", owner);
            return;
        }

        cooldownTime += deltaTime;
    }

    public float GetCooldownRemaining(CombatActionDefinition action)
    {
        if (action == null || !cooldownEndTimes.TryGetValue(action, out double endTime)) return 0f;
        return (float)Math.Max(0d, endTime - cooldownTime);
    }

    public void StartCooldown(CombatActionDefinition action)
    {
        if (action == null || currentAction != action)
        {
            Debug.LogWarning($"[CombatAction] {OwnerName}: only the active action can begin cooldown; cooldown not recorded.", owner);
            return;
        }

        float duration = action.CooldownDuration;
        if (!(duration >= 0f) || float.IsInfinity(duration))
        {
            Debug.LogWarning($"[CombatAction] {OwnerName}: {action.name} has an invalid cooldown duration; cooldown not recorded.", owner);
            return;
        }

        if (duration == 0f) cooldownEndTimes.Remove(action);
        else cooldownEndTimes[action] = cooldownTime + duration;
    }

    public CombatActionUsageRuntime UsageRuntime => usageRuntime;
    public CombatActionChainRuntime ChainRuntime => chainRuntime;
    public bool IsHitWindowOpen => activeHitWindow != null;
    public int ActiveHitWindowId => activeHitWindow != null ? activeHitWindow.WindowId : -1;
    protected bool HasPendingAction => pendingAction != null;
    public event Action HitWindowEnded;

    public bool TryStartAction(CombatActionDefinition action, in CombatActionContext context)
    {
        if (!TryPrepareEntryAction(action, in context)) return false;
        BeginAction(action);
        return true;
    }

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
            // Input-driven branches belong to the player and are not automatic candidates.
            if (transition.Command != null) continue;
            if (transition.TargetAction == null)
            {
                Debug.LogWarning("Target action is null");
                return false;
            }
            if (transition.AreConditionsMet(context) == false) continue;
            if (transition.TargetAction.AreEntryConditionsMet(context) == false) continue;
            if (!TryPrepareStage(transition.TargetAction, in context)) continue;
            RequestTransitionCommit();
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
        isAutomaticTransitionWindowOpen = false;
        isTransitionCommitRequested = false;
        activeCancelWindow = null;
        chainRuntime.Clear();
        OnActionRuntimeCleared();
    }

    protected void RequestTransitionCommit()
    {
        isTransitionCommitRequested = true;
    }

    protected bool TryPrepareEntryAction(CombatActionDefinition action, in CombatActionContext context)
    {
        if (currentAction != null) return false;
        if (action == null)
        {
            Debug.LogWarning($"[CombatAction] {OwnerName}: target action is missing; action not prepared.", owner);
            return false;
        }
        if (!action.AreEntryConditionsMet(in context)) return false;
        return TryPrepareStage(action, in context);
    }

    protected bool TryPrepareStage(CombatActionDefinition action, in CombatActionContext context)
    {
        if (!(action.CooldownDuration >= 0f) || float.IsInfinity(action.CooldownDuration))
        {
            Debug.LogWarning($"[CombatAction] {OwnerName}: {action.name} has an invalid cooldown duration; action not prepared.", owner);
            return false;
        }
        if (GetCooldownRemaining(action) > 0f) return false;
        if (!action.TrySelectStage(in context, out var stage)) return false;
        if (stage is not CombatActionStage combatStage || string.IsNullOrWhiteSpace(stage.AnimatorStateName))
        {
            Debug.LogWarning($"[CombatAction] {OwnerName}: selected stage in {action.name} has no valid combat animation; action not prepared.", owner);
            return false;
        }
        pendingAction = action;
        pendingStage = combatStage;
        return true;
    }

    protected virtual void OnActionRuntimeCleared()
    {
    }

    protected virtual void OnActionStarted()
    {
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
            Debug.LogWarning($"[CombatAction] {OwnerName}: {action.name} has no pending stage; action not started.", owner);
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
        isAutomaticTransitionWindowOpen = false;
        isTransitionCommitRequested = false;
        activeCancelWindow = null;
        OnActionStarted();
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


    protected override void InterruptCurrentAction()
    {
        CancelCurrentAction();
    }

    public void NotifyStageFinished(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        isCurrentStageFinished = true;
    }
}
