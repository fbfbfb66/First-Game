using UnityEngine;

public abstract class ActionController
{
    protected readonly UnityEngine.Object owner;
    protected string OwnerName => owner != null ? owner.name : "<missing owner>";

    protected ActionController(UnityEngine.Object owner)
    {
        this.owner = owner != null ? owner : throw new System.ArgumentNullException(nameof(owner));
    }

    protected CombatActionCancelWindowDefinition activeCancelWindow;

    protected abstract ActionDefinition ActiveAction { get; }
    public abstract ActionStage CurrentStage { get; }
    public bool IsActionRunning => ActiveAction != null;
    public CombatActionCancelWindowDefinition ActiveCancelWindow => activeCancelWindow;
    public int ActiveCancelWindowId => activeCancelWindow != null ? activeCancelWindow.WindowId : -1;
    public bool IsCancelWindowOpen => activeCancelWindow != null;

    public bool AllowsCancel(ActionCommandKey command)
    {
        if (command == null) return false;
        return !IsActionRunning || activeCancelWindow != null && activeCancelWindow.Accepts(command);
    }

    public bool CanInterrupt(ActionCommandKey command = null)
    {
        return !IsActionRunning || (command != null ? AllowsCancel(command) : IsCancelWindowOpen);
    }

    public bool TryInterrupt(ActionCommandKey command = null)
    {
        if (!CanInterrupt(command)) return false;
        if (IsActionRunning) InterruptCurrentAction();
        return true;
    }

    protected abstract void InterruptCurrentAction();

    public void NotifyCancelWindowOpened(int windowId, int sourceStateHash)
    {
        if (!IsSignalFromCurrentAction(sourceStateHash)) return;
        if (!CurrentStage.TryGetCancelWindow(windowId, out var window)) return;
        if (activeCancelWindow != null && activeCancelWindow != window)
        {
            Debug.LogWarning($"[Action] {OwnerName}: cannot open window {windowId}; window {ActiveCancelWindowId} is still open.", owner);
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
