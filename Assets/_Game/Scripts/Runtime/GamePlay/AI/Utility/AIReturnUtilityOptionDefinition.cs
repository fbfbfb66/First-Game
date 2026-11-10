using UnityEngine;
[CreateAssetMenu(fileName = "UtilityOption_Return", menuName = "Game/AI/Utility/Return Option")]
public class AIReturnUtilityOptionDefinition : AIUtilityOptionDefinition
{
    [SerializeField] private AIIntentKey intentKey;
    [SerializeField] private AIFlyToIdlePointTaskDefinition returnTask;

    public AIIntentKey IntentKey => intentKey;

    public override bool IsFeasible(in AIAgentContext context)
    {
        if (context.Owner == null || context.Area == null ||
            returnTask == null || !context.IdlePosition.HasValue) return false;
        if (!float.IsFinite(returnTask.StoppingDistance) || returnTask.StoppingDistance < 0f ||
            !float.IsFinite(context.IdlePosition.Value.x) || !float.IsFinite(context.IdlePosition.Value.y)) return false;
        if (IsAtReturnPoint(in context)) return false;
        if (context.Target == null) return true;
        if (!context.Area.Contains(context.Owner.position)) return true;
        if (!context.Area.ContainsTarget(context.Target.position)) return true;

        float distance = Vector2.Distance(context.Owner.position, context.Target.position);
        float maxRange = context.Area.DiagonalLength;
        return distance > maxRange && !Mathf.Approximately(distance, maxRange);
    }

    public override bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation evaluation)
    {
        evaluation = default;
        if (intentKey == null || context.Owner == null || context.Area == null ||
            returnTask == null || !context.IdlePosition.HasValue)
        {
            Debug.LogWarning($"[AIUtility] {name}: return requires an intent key, owner, active area, return task and idle position; evaluation failed.", this);
            return false;
        }
        if (!float.IsFinite(returnTask.StoppingDistance) || returnTask.StoppingDistance < 0f ||
            !float.IsFinite(context.IdlePosition.Value.x) || !float.IsFinite(context.IdlePosition.Value.y))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: return position must be finite and stopping distance finite and non-negative; evaluation failed.", this);
            return false;
        }
        if (!float.IsFinite(context.Area.DiagonalLength) || context.Area.DiagonalLength <= 0f)
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: active area diagonal must be finite and positive; return evaluation failed.", this);
            return false;
        }

        bool feasible = IsFeasible(in context);
        evaluation = new AIUtilityEvaluation(new AIIntent(intentKey, null), feasible, feasible ? 1f : 0f,
            feasible
                ? "Return available: owner/target outside active area, chase range exceeded, or no detected target."
                : IsAtReturnPoint(in context)
                    ? "Return unavailable: owner has reached the return point."
                    : "Return unavailable: target detected and owner/target remain within active area and chase range.");
        return true;
    }

    private bool IsAtReturnPoint(in AIAgentContext context)
    {
        float distance = Vector2.Distance(context.Owner.position, context.IdlePosition.Value);
        return distance <= returnTask.StoppingDistance || Mathf.Approximately(distance, returnTask.StoppingDistance);
    }

}
