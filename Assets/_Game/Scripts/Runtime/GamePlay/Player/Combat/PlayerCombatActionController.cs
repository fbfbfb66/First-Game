using UnityEngine;

public class PlayerCombatActionController : MonoBehaviour
{
    [SerializeField] private CombatActionDefinition defaultAction;
    [SerializeField] private CombatActionSystemConfig config;
    private CombatActionRequest bufferedRequest;
    private bool hasBufferedRequest;
    private bool isCurrentStageFinished;
    private bool isTransitionWindowOpen;
    private bool isTransitionCommitRequested;
    private CombatActionDefinition currentAction;
    private CombatActionDefinition pendingTransitionAction;
    private CombatActionCancelWindowDefinition activeCancelWindow;
    private CombatHitWindowDefinition activeHitWindow;

    public bool IsActionRunning => currentAction != null;
    public CombatActionCancelWindowDefinition ActiveCancelWindow => activeCancelWindow;
    public bool IsCancelWindowOpen => activeCancelWindow != null;
    public bool IsHitWindowOpen => activeHitWindow != null;
    public int ActiveCancelWindowId => activeCancelWindow.WindowId;
    public int ActiveHitWindowId => activeHitWindow.WindowId;
    public CombatActionDefinition CurrentAction => currentAction;
    public bool HasBufferedRequest => hasBufferedRequest;
    public bool IsTransitionWindowOpen => isTransitionWindowOpen;
    public CombatActionRequest BufferedRequest => bufferedRequest;

    public void RequestAction(CombatActionCommandKey command)
    {
        if (command == null)
        {
            Debug.LogWarning($"[CombatAction] Command is missing on {name}. Request ignored.", this);
            return;
        }
        bufferedRequest = new CombatActionRequest(command, Time.time);
        hasBufferedRequest = true;
        Debug.Log($"[CombatAction] Request buffered | Command={command.name} | RequestedTime={bufferedRequest.RequestedTime:F3}");
    }

    public bool TryCommitTransition(out CombatActionDefinition targetAction)
    {
        targetAction = null;
        if (isTransitionCommitRequested == false) return false;
        isTransitionCommitRequested = false;
        if (pendingTransitionAction == null) return false;
        targetAction = pendingTransitionAction;
        pendingTransitionAction = null;
        return true;
    }

    private void ClearCurrentActionRuntime()
    {
        currentAction = null;
        isCurrentStageFinished = false;
        isTransitionWindowOpen = false;
        hasBufferedRequest = false;
        isTransitionCommitRequested = false;
        pendingTransitionAction = null;
        activeHitWindow = null;
        activeCancelWindow = null;
        bufferedRequest = default;
    }

    private bool TryGetValidBufferedRequest(out CombatActionRequest request)
    {
        request = default;
        if (hasBufferedRequest == false) return false;
        if (config == null)
        {
            Debug.LogWarning($"[CombatAction] System config is missing on {name}. Buffered request cannot be validated.", this);
            return false;
        }
        if (Time.time - bufferedRequest.RequestedTime > config.InputBufferDuration)
        {
            hasBufferedRequest = false;
            return false;
        }
        request = bufferedRequest;
        return true;
    }

    public bool TryQueueTransition()
    {
        if (currentAction == null) return false;
        if (TryGetValidBufferedRequest(out var request) == false) return false;
        if (isTransitionWindowOpen == false) return false;
        CombatActionTransition transition = currentAction.Transition;
        if (transition == null) return false;
        if (transition.Command == null) return false;
        if (transition.TargetAction == null) return false;
        if (transition.TargetAction.Stage == null || string.IsNullOrWhiteSpace(transition.TargetAction.Stage.AnimatorStateName))
        {
            Debug.LogWarning($"[CombatAction] Transition target {transition.TargetAction.name} has no valid stage.", transition.TargetAction);
            return false;
        }
        if (request.Command != transition.Command) return false;
        if (pendingTransitionAction != null) return false;
        pendingTransitionAction = transition.TargetAction;
        hasBufferedRequest = false;
        return true;
    }

    public bool TryPrepareCancel(out CombatActionRequest request)
    {
        request = default;
        if (currentAction == null) return false;
        if (TryGetValidBufferedRequest(out var tempRequest) == false) return false;
        if (activeCancelWindow == null) return false;
        if (activeCancelWindow.Accepts(tempRequest.Command) == false) return false;
        request = tempRequest;
        return true;
    }

    public bool TryGetEntryActionCandidate(out CombatActionDefinition action)
    {
        action = null;
        if (currentAction != null) return false;
        if (TryGetValidBufferedRequest(out var request) == false) return false;
        if (defaultAction == null)
        {
            Debug.LogWarning("Missed defaultAction");
            return false;
        }
        if (defaultAction.Command == null)
        {
            Debug.LogWarning("Missed command in defaultAction");
            return false;
        }
        if (defaultAction.Command != request.Command) return false;
        if (defaultAction.Stage == null || string.IsNullOrWhiteSpace(defaultAction.Stage.AnimatorStateName)) return false;

        action = defaultAction;
        Debug.Log($"Action Prepared : {action.name},{action.Command.name},{bufferedRequest.RequestedTime}");

        return true;
    }

    public bool TryConsumeBufferedRequest(CombatActionCommandKey expectedCommand)
    {
        if(expectedCommand == null) return false;
        if (TryGetValidBufferedRequest(out var request) == false) return false;
        if (request.Command != expectedCommand) return false;
        hasBufferedRequest = false;
        bufferedRequest = default;
        Debug.Log("Request consumed");
        return true;
    }

    public void CancelCurrentAction()
    {
        if (currentAction == null) return;
        string actionName = currentAction.Stage.AnimatorStateName;
        ClearCurrentActionRuntime();
        Debug.Log($"Action Canceled :{actionName}");
    }

    public bool TryCompleteAction()
    {
        if (currentAction == null) return false;
        if (isCurrentStageFinished == false) return false;
        string actionName = currentAction.Stage.AnimatorStateName;
        ClearCurrentActionRuntime();
        Debug.Log($"{actionName} finished");
        return true;
    }

    public void BeginAction(CombatActionDefinition action)
    {
        if (action == null)
        {
            Debug.LogWarning("action is null");
            return;
        }
        currentAction = action;
        pendingTransitionAction = null;
        isCurrentStageFinished = false;
        isTransitionWindowOpen = false;
        isTransitionCommitRequested = false;
        activeHitWindow = null;
        activeCancelWindow = null;
    }

    public void NotifyCancelWindowOpened(int windowId, int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        if (activeCancelWindow != null)
        {
            Debug.LogWarning("ActiveCancelWindow already Exit");
            return;
        }
        if (currentAction.Stage.TryGetCancelWindow(windowId, out var cancelWindow) == false) return;
        activeCancelWindow = cancelWindow;
        Debug.Log($"Action : {CurrentAction.Stage.AnimatorStateName},CancelWindowId :{windowId} opened");
    }

    public void NotifyCancelWindowClosed(int windowId, int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        if (activeCancelWindow == null)
        {
            Debug.LogWarning("ActiveCancelWindow Not Exit");
            return;
        }
        if (ActiveCancelWindowId != windowId) return;
        activeCancelWindow = null;
        Debug.Log($"Action : {CurrentAction.Stage.AnimatorStateName},CancelWindowId :{windowId} closed");
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
        if (currentAction.Stage.TryGetHitWindow(windowId, out var hitWindow) == false)
        {
            Debug.LogWarning($"Action : {CurrentAction.Stage.AnimatorStateName} didn`t find hitWindow compared whih {windowId}");
            return;
        }

        activeHitWindow = hitWindow;
        Debug.Log($"Action : {CurrentAction.Stage.AnimatorStateName},WindowId :{windowId} opened");

    }
    public void NotifyHitWindowClosed(int windowId,int sourceStateHash)
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
        activeHitWindow = null;
        Debug.Log($"Action : {CurrentAction.Stage.AnimatorStateName},WindowId :{windowId} closed");
    }

    private bool IsSignalFromCurrentAction(int sourceStateHash)
    {
        if (currentAction == null) return false;
        if (currentAction.Stage == null) return false;
        if (currentAction.Stage.AnimatorStateHash != sourceStateHash) return false;
        return true;
    }

    public void NotifyTransitionCommitRequested(int sourceStateHash)
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
        Debug.Log($"Transition window closed : Action[{currentAction.Stage.AnimatorStateName}]");
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
        Debug.Log($"Transition window opened : Action[{currentAction.Stage.AnimatorStateName}]");
    }

    public void NotifyStageFinished(int sourceStateHash)
    {
        if (IsSignalFromCurrentAction(sourceStateHash) == false) return;
        isCurrentStageFinished = true;
    }
}
