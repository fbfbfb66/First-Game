using UnityEngine;

public class PlayerMotionController : ActionController
{
    private MotionActionDefinition currentAction;
    private int currentAnimatorStateHash;
    private bool hasActiveState;

    protected override ActionDefinition ActiveAction => currentAction;
    public MotionActionDefinition CurrentAction => currentAction;

    public void BeginState(int animatorStateHash, MotionActionDefinition action)
    {
        currentAnimatorStateHash = animatorStateHash;
        hasActiveState = true;
        currentAction = action;
        activeCancelWindow = null;
    }

    public void EndState(int animatorStateHash)
    {
        if (!IsCurrentStateAnimation(animatorStateHash)) return;
        hasActiveState = false;
        currentAction = null;
        activeCancelWindow = null;
    }

    public bool IsCurrentStateAnimation(int animatorStateHash)
    {
        return hasActiveState && currentAnimatorStateHash == animatorStateHash;
    }

    public bool TryStartAction(MotionActionDefinition action, in ActionContext context)
    {
        if (action == null || action.Stage == null || string.IsNullOrWhiteSpace(action.Stage.AnimatorStateName))
        {
            Debug.LogWarning($"[Motion] {name}: target action or animation is missing; request not consumed.", this);
            return false;
        }
        if (action == currentAction) return false;
        if (IsActionRunning && !AllowsCancel(action.Command)) return false;
        if (action.NeedCommand)
        {
            if (!TryGetValidBufferedRequest(out var request) || action.Command == null || request.Command != action.Command)
                return false;
        }
        if (!action.AreEntryConditionsMet(in context)) return false;

        if (action.NeedCommand)
        {
            hasBufferedRequest = false;
            bufferedRequest = default;
        }
        // The FSM Enter/Exit callbacks register the executed state after the transition commits.
        return true;
    }
}
