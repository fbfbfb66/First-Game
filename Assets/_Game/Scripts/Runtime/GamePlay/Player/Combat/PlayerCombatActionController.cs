using UnityEngine;

public class PlayerCombatActionController : CombatActionController
{
    [SerializeField] private ActionSystemConfig config;
    [SerializeField] private CombatActionDefinition[] entryActions;
    private readonly PlayerActionRequestBuffer requestBuffer = new();
    private bool isTransitionWindowOpen;

    public bool IsTransitionWindowOpen => isTransitionWindowOpen;
    public bool HasBufferedRequest => requestBuffer.HasBufferedRequest;
    public ActionRequest BufferedRequest => requestBuffer.BufferedRequest;
    public float RequestBufferDuration => config != null ? config.InputBufferDuration : 0f;

    public void RequestAction(ActionCommandKey command) => requestBuffer.RequestAction(command, this);

    public bool TryGetValidBufferedRequest(out ActionRequest request)
        => requestBuffer.TryGetValidBufferedRequest(config, this, out request);

    public bool TryConsumeBufferedRequest(ActionCommandKey expectedCommand)
        => requestBuffer.TryConsumeBufferedRequest(expectedCommand, config, this);

    public bool TryGetEntryActionCandidate(in CombatActionContext context, out CombatActionDefinition action)
    {
        action = null;
        if (IsActionRunning || !TryGetValidBufferedRequest(out var request)) return false;
        if (entryActions == null || entryActions.Length == 0)
        {
            Debug.LogWarning($"[CombatAction] {name}: entry actions are missing; request not consumed.", this);
            return false;
        }

        foreach (var entry in entryActions)
        {
            if (entry == null || entry.Command == null)
            {
                Debug.LogWarning($"[CombatAction] {name}: an entry action or its command is missing; entry skipped.", this);
                continue;
            }

            if (entry.Command != request.Command || !TryPrepareEntryAction(entry, in context)) continue;
            action = entry;
            return true;
        }

        return false;
    }

    public bool TryQueueTransition(in CombatActionContext context)
    {
        if (CurrentAction == null || HasPendingAction || !isTransitionWindowOpen) return false;
        if (!TryGetValidBufferedRequest(out var request)) return false;
        var transitions = CurrentAction.Transitions;
        if (transitions == null) return false;
        foreach (var transition in transitions)
        {
            // Automatic branches are handled separately by the shared automatic flow.
            if (transition == null || transition.Command == null || transition.Command != request.Command) continue;
            if (transition.TargetAction == null)
            {
                Debug.LogWarning($"[CombatAction] {name}: an input transition in {CurrentAction.name} has no target; transition skipped.", this);
                continue;
            }
            if (!transition.AreConditionsMet(in context)) continue;
            if (!transition.TargetAction.AreEntryConditionsMet(in context)) continue;
            if (!TryPrepareStage(transition.TargetAction, in context)) continue;
            requestBuffer.Clear();
            return true;
        }
        return false;
    }

    public void NotifyTransitionWindowOpened(int sourceStateHash)
    {
        if (!IsSignalFromCurrentAction(sourceStateHash)) return;
        isTransitionWindowOpen = true;
    }

    public void NotifyTransitionWindowClosed(int sourceStateHash)
    {
        if (!IsSignalFromCurrentAction(sourceStateHash)) return;
        isTransitionWindowOpen = false;
    }

    public void NotifyTransitionCommitOpened(int sourceStateHash)
    {
        if (!IsSignalFromCurrentAction(sourceStateHash)) return;
        isTransitionWindowOpen = false;
        RequestTransitionCommit();
    }

    protected override void OnActionStarted()
    {
        base.OnActionStarted();
        isTransitionWindowOpen = false;
    }

    protected override void OnActionRuntimeCleared()
    {
        base.OnActionRuntimeCleared();
        isTransitionWindowOpen = false;
        requestBuffer.Clear();
    }
}
