using UnityEngine;

public abstract class AIUtilityOptionDefinition : ScriptableObject
{
    // Evaluation results belong to the caller; shared resources must not retain per-character state.
    // Return true for a completed evaluation, including an infeasible evaluation.
    // Return false when evaluation cannot be completed, such as invalid configuration.
    public abstract bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation evaluation);

    public virtual bool IsFeasible(in AIAgentContext context)
    {
        if (context.Owner == null || context.Target == null || context.Area == null) return false;

        return context.Area.Contains(context.Owner.position) && context.Area.ContainsTarget(context.Target.position);
    }
}
