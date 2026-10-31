using UnityEngine;

public abstract class ActionController : MonoBehaviour
{
    [SerializeField] private ActionSystemConfig config;
    protected CombatActionCancelWindowDefinition activeCancelWindow;
    protected ActionRequest bufferedRequest;
    protected bool hasBufferedRequest;

    protected abstract ActionDefinition ActiveAction { get; }
    public abstract ActionStage CurrentStage { get; }
    public bool IsActionRunning => ActiveAction != null;
    public CombatActionCancelWindowDefinition ActiveCancelWindow => activeCancelWindow;
    public int ActiveCancelWindowId => activeCancelWindow != null ? activeCancelWindow.WindowId : -1;
    public bool IsCancelWindowOpen => activeCancelWindow != null;
    public bool HasBufferedRequest => hasBufferedRequest;
    public ActionRequest BufferedRequest => bufferedRequest;
    public float RequestBufferDuration => config != null ? config.InputBufferDuration : 0f;

    public void RequestAction(ActionCommandKey command)
    {
        if (command == null)
        {
            Debug.LogWarning($"[Action] {name}: command is missing; request ignored.", this);
            return;
        }
        bufferedRequest = new ActionRequest(command, Time.time);
        hasBufferedRequest = true;
    }

    public bool TryConsumeBufferedRequest(ActionCommandKey expectedCommand)
    {
        if (expectedCommand == null) return false;
        if (!TryGetValidBufferedRequest(out var request) || request.Command != expectedCommand) return false;
        hasBufferedRequest = false;
        bufferedRequest = default;
        return true;
    }

    public bool TryGetValidBufferedRequest(out ActionRequest request)
    {
        request = default;
        if (!hasBufferedRequest) return false;
        if (config == null)
        {
            Debug.LogWarning($"[Action] {name}: system config is missing; request cannot be validated.", this);
            return false;
        }
        if (Time.time - bufferedRequest.RequestedTime > config.InputBufferDuration)
        {
            hasBufferedRequest = false;
            bufferedRequest = default;
            return false;
        }
        request = bufferedRequest;
        return true;
    }

    public bool AllowsCancel(ActionCommandKey command)
    {
        if (command == null) return false;
        return !IsActionRunning || activeCancelWindow != null && activeCancelWindow.Accepts(command);
    }

    public void NotifyCancelWindowOpened(int windowId, int sourceStateHash)
    {
        if (!IsSignalFromCurrentAction(sourceStateHash)) return;
        if (!CurrentStage.TryGetCancelWindow(windowId, out var window)) return;
        if (activeCancelWindow != null && activeCancelWindow != window)
        {
            Debug.LogWarning($"[Action] {name}: cannot open window {windowId}; window {ActiveCancelWindowId} is still open.", this);
            return;
        }
        activeCancelWindow = window;
    }

    public void NotifyCancelWindowClosed(int windowId, int sourceStateHash)
    {
        if (!IsSignalFromCurrentAction(sourceStateHash)) return;
        if (activeCancelWindow == null || activeCancelWindow.WindowId != windowId) return;
        activeCancelWindow = null;
    }

    protected bool IsSignalFromCurrentAction(int sourceStateHash)
    {
        return ActiveAction != null && CurrentStage != null &&
            CurrentStage.AnimatorStateHash == sourceStateHash;
    }
}
