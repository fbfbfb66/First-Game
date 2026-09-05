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

    private int activeHitWindowId = -1;

    public bool IsHitWindowOpen { get; private set; }
    public int ActiveHitWindowId => activeHitWindowId;
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
        if(pendingTransitionAction  == null) return false;
        targetAction = pendingTransitionAction;
        pendingTransitionAction = null;
        return true;
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
        if(transition.Command == null) return false;
        if(transition.TargetAction == null) return false;
        if (transition.TargetAction.Stage == null || string.IsNullOrWhiteSpace(transition.TargetAction.Stage.AnimatorStateName))
        {
            Debug.LogWarning($"[CombatAction] Transition target {transition.TargetAction.name} has no valid stage.", transition.TargetAction);
            return false;
        }
        if(request.Command != transition.Command) return false;
        if(pendingTransitionAction != null) return false;
        pendingTransitionAction = transition.TargetAction;
        hasBufferedRequest = false;
        return true;
    }

    public bool TryPrepareAction(out CombatActionDefinition action)
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
        hasBufferedRequest = false;
        Debug.Log($"Action Prepared : {action.name},{action.Command.name},{bufferedRequest.RequestedTime}");

        return true;
    }

    public bool TryCompleteAction()
    {
        if (currentAction == null) return false;
        if (isCurrentStageFinished == false) return false;
        string actionName = currentAction.Stage.AnimatorStateName;
        currentAction = null;
        isCurrentStageFinished = false;
        isTransitionWindowOpen = false;
        hasBufferedRequest = false;
        isTransitionCommitRequested = false;
        pendingTransitionAction = null;
        activeHitWindowId = -1;
        IsHitWindowOpen = false;
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
        activeHitWindowId = -1;
        IsHitWindowOpen = false;
    }

    public void NotifyHitWindowOpened(int windowId)
    {
        if(currentAction == null)
        {
            Debug.LogWarning("CurrentAction is null");
            return;
        }
        if(windowId < 0)
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

        IsHitWindowOpen = true;
        activeHitWindowId = windowId;
        Debug.Log($"Action : {CurrentAction.Stage.AnimatorStateName},WindowId :{windowId} opened");

    }
    public void NotifyHitWindowClosed(int windowId)
    {
        if (currentAction == null)
        {
            Debug.LogWarning("CurrentAction is null");
            return;
        }
        if (IsHitWindowOpen==false)
        {
            Debug.LogWarning("HitWindow Not Exit");
            return;
        }
        if(windowId != activeHitWindowId)
        {
            Debug.LogWarning("WindowId != activeHitWindowId");
            return;
        }
        IsHitWindowOpen=false;
        activeHitWindowId = -1;
        Debug.Log($"Action : {CurrentAction.Stage.AnimatorStateName},WindowId :{windowId} closed");
    }

    public void NotifyTransitionCommitRequested()
    {
        if (currentAction == null)
        {
            Debug.Log("CurrentAction is null");
            return;
        }
        isTransitionWindowOpen = false;
        isTransitionCommitRequested = true;
    }

    public void NotifyTransitionWindowClosed()
    {
        if (currentAction == null)
        {
            Debug.Log("CurrentAction is null");
            return;
        }
        if (isTransitionWindowOpen == false)
        {
            Debug.LogWarning("window has closed");
            return;
        }
        isTransitionWindowOpen = false;
        Debug.Log($"Transition window closed : Action[{currentAction.Stage.AnimatorStateName}]");
    }

    public void NotifyTransitionWindowOpened()
    {
        if (currentAction == null)
        {
            Debug.Log("CurrentAction is null");
            return;
        }
        if (isTransitionWindowOpen)
        {
            Debug.LogWarning("window has opened");
            return;
        }
        isTransitionWindowOpen = true;
        Debug.Log($"Transition window opened : Action[{currentAction.Stage.AnimatorStateName}]");
    }

    public void NotifyStageFinished()
    {
        if (currentAction == null)
        {
            Debug.Log("CurrentAction is null");
            return;
        }
        isCurrentStageFinished = true;
    }
}
