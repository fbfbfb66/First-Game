using UnityEngine;
using System.Collections.Generic;

public class MotionController : ActionController
{
    public MotionController(UnityEngine.Object owner) : base(owner) { }

    private MotionActionDefinition currentAction;
    private int currentAnimatorStateHash;
    private bool hasActiveState;
    private ActionStage currentStage;
    private MotionActionDefinition pendingAction;
    private ActionStage pendingStage;
    private readonly List<MotionStageEffectInstance> activeEffects = new();

    protected override ActionDefinition ActiveAction => currentAction;
    public MotionActionDefinition CurrentAction => currentAction;
    public override ActionStage CurrentStage => currentStage;

    public int BeginState(int animatorStateHash, MotionActionDefinition action, in ActionContext context)
    {
        ActionStage selectedStage = null;
        if (action != null)
        {
            if (pendingAction == action) selectedStage = pendingStage;
            else action.TrySelectStage(in context, out selectedStage);
        }
        return CommitState(animatorStateHash, action, selectedStage);
    }

    public void EndState(int animatorStateHash)
    {
        if (!IsCurrentStateAnimation(animatorStateHash)) return;
        EndStageEffects();
        hasActiveState = false;
        currentAction = null;
        currentStage = null;
        activeCancelWindow = null;
    }

    protected override void InterruptCurrentAction()
    {
        // Keep the prepared next stage for the subsequent FSM Enter callback.
        EndState(currentAnimatorStateHash);
    }

    public bool IsCurrentStateAnimation(int animatorStateHash)
    {
        return hasActiveState && currentAnimatorStateHash == animatorStateHash;
    }

    // Prepares the action; the FSM commits it through BeginState after switching states.
    public virtual bool TryStartAction(MotionActionDefinition action, in ActionContext context)
    {
        if (action == null)
        {
            Debug.LogWarning($"[Motion] {OwnerName}: target action is missing; action not prepared.", owner);
            return false;
        }
        if (action == currentAction) return false;
        if (!CanInterrupt(action.Command)) return false;
        if (!action.AreEntryConditionsMet(in context)) return false;
        if (!action.TrySelectStage(in context, out var stage)) return false;
        if (string.IsNullOrWhiteSpace(stage.AnimatorStateName))
        {
            Debug.LogWarning($"[Motion] {OwnerName}: selected stage in {action.name} has no animation; action not prepared.", owner);
            return false;
        }

        pendingAction = action;
        pendingStage = stage;
        // The FSM Enter/Exit callbacks register the executed state after the transition commits.
        return true;
    }


    public int BeginState(int animatorStateHash, MotionActionDefinition action, in AIActionContext context)
    {
        ActionStage selectedStage = null;
        if (action != null)
        {
            if (pendingAction == action) selectedStage = pendingStage;
            else action.TrySelectStage(in context, out selectedStage);
        }
        return CommitState(animatorStateHash, action, selectedStage);
    }

    public bool TryStartAction(MotionActionDefinition action, in AIActionContext context)
    {
        if (action == null)
        {
            Debug.LogWarning($"[Motion] {OwnerName}: target action is missing; action not prepared.", owner);
            return false;
        }
        if (action == currentAction) return false;
        if (!CanInterrupt(action.Command)) return false;
        if (!action.AreEntryConditionsMet(in context)) return false;
        if (!action.TrySelectStage(in context, out var stage)) return false;
        if (string.IsNullOrWhiteSpace(stage.AnimatorStateName))
        {
            Debug.LogWarning($"[Motion] {OwnerName}: selected stage in {action.name} has no animation; action not prepared.", owner);
            return false;
        }

        pendingAction = action;
        pendingStage = stage;
        // The FSM Enter/Exit callbacks register the executed state after the transition commits.
        return true;
    }

    private int CommitState(int animatorStateHash, MotionActionDefinition action, ActionStage stage)
    {
        EndStageEffects();
        currentStage = stage;
        if (action != null)
        {
            if (stage != null && !string.IsNullOrWhiteSpace(stage.AnimatorStateName))
                animatorStateHash = stage.AnimatorStateHash;
            else Debug.LogWarning($"[Motion] {OwnerName}: {action.name} has no valid stage on entry; using the state's animation.", owner);
        }
        pendingAction = null;
        pendingStage = null;
        currentAnimatorStateHash = animatorStateHash;
        hasActiveState = true;
        currentAction = action;
        activeCancelWindow = null;
        BeginStageEffects();
        return animatorStateHash;
    }

    private void BeginStageEffects()
    {
        var actor = owner is Component component ? component.gameObject : owner as GameObject;
        if (currentStage?.Effects == null) return;
        foreach (var effect in currentStage.Effects)
        {
            if (effect == null) continue;
            var instance = effect.CreateInstance(actor);
            if (instance == null) continue;
            activeEffects.Add(instance);
            instance.Begin();
        }
    }

    private void EndStageEffects()
    {
        // Reverse order also correctly restores repeated effects within one stage.
        while (activeEffects.Count > 0)
        {
            int index = activeEffects.Count - 1;
            var instance = activeEffects[index];
            activeEffects.RemoveAt(index);
            instance.End();
        }
    }
}
