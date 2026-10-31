using UnityEngine;

public class PlayerMotionController : ActionController
{
    private MotionActionDefinition currentAction;
    private int currentAnimatorStateHash;
    private bool hasActiveState;
    private ActionStage currentStage;
    private MotionActionDefinition pendingAction;
    private ActionStage pendingStage;

    protected override ActionDefinition ActiveAction => currentAction;
    public MotionActionDefinition CurrentAction => currentAction;
    public override ActionStage CurrentStage => currentStage;

    public int BeginState(int animatorStateHash, MotionActionDefinition action, in ActionContext context)
    {
        currentStage = null;
        if (action != null)
        {
            if (pendingAction == action)
                currentStage = pendingStage;
            else
                action.TrySelectStage(in context, out currentStage);

            if (currentStage != null && !string.IsNullOrWhiteSpace(currentStage.AnimatorStateName))
                animatorStateHash = currentStage.AnimatorStateHash;
            else
                Debug.LogWarning($"[Motion] {name}: {action.name} has no valid stage on state entry; using the state's animation.", this);
        }
        pendingAction = null;
        pendingStage = null;
        currentAnimatorStateHash = animatorStateHash;
        hasActiveState = true;
        currentAction = action;
        activeCancelWindow = null;
        return animatorStateHash;
    }

    public void EndState(int animatorStateHash)
    {
        if (!IsCurrentStateAnimation(animatorStateHash)) return;
        hasActiveState = false;
        currentAction = null;
        currentStage = null;
        activeCancelWindow = null;
    }

    public bool IsCurrentStateAnimation(int animatorStateHash)
    {
        return hasActiveState && currentAnimatorStateHash == animatorStateHash;
    }

    public bool TryStartAction(MotionActionDefinition action, in ActionContext context)
    {
        if (action == null)
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
        if (!action.TrySelectStage(in context, out var stage)) return false;
        if (string.IsNullOrWhiteSpace(stage.AnimatorStateName))
        {
            Debug.LogWarning($"[Motion] {name}: selected stage in {action.name} has no animation; request not consumed.", this);
            return false;
        }

        pendingAction = action;
        pendingStage = stage;
        if (action.NeedCommand)
        {
            hasBufferedRequest = false;
            bufferedRequest = default;
        }
        // The FSM Enter/Exit callbacks register the executed state after the transition commits.
        return true;
    }
}
