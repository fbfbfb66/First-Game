using UnityEngine;

[CreateAssetMenu(fileName = "UtilityOption_Attack", menuName = "Game/AI/Enemies/Utility/Attack Option")]
public class AIAttackUtilityOptionDefinition : AIUtilityOptionDefinition
{
    [SerializeField] private AIIntentKey intentKey;
    [SerializeField] private CombatActionDefinition action;
    [SerializeField] private float maxRange = 2f;
    [SerializeField] private AnimationCurve distanceUtilityCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    public AIIntentKey IntentKey => intentKey;
    public CombatActionDefinition Action => action;
    public float MaxRange => maxRange;
    public AnimationCurve DistanceUtilityCurve => distanceUtilityCurve;

    public override bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation evaluation)
    {
        evaluation = default;
        if (context.Owner == null || intentKey == null || action == null ||
            context.CombatExecutor == null || context.CombatExecutor.CombatActionController == null || context.FlightMotor == null)
        {
            Debug.LogWarning($"[AIUtility] {name}: owner, intent key, action, Combat controller or facing motor is missing; attack evaluation failed.", this);
            return false;
        }

        if (!(maxRange > 0f) || float.IsInfinity(maxRange))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: maxRange={maxRange} must be finite and positive; attack evaluation failed.", this);
            return false;
        }

        if (distanceUtilityCurve == null || distanceUtilityCurve.length == 0)
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: distance utility curve is missing or empty; attack evaluation failed.", this);
            return false;
        }

        if (!(action.CooldownDuration >= 0f) || float.IsInfinity(action.CooldownDuration))
        {
            Debug.LogWarning($"[AIUtility] {name}: action '{action.name}' has an invalid cooldown duration; attack evaluation failed.", this);
            return false;
        }

        float remaining = context.CombatExecutor.CombatActionController.GetCooldownRemaining(action);
        if (remaining > 0f)
        {
            evaluation = new AIUtilityEvaluation(new AIIntent(intentKey, context.Target), false, 0f,
                $"Attack infeasible: action '{action.name}' has {remaining:F3}s of cooldown remaining.");
            return true;
        }

        Transform target = context.Target;
        if (target == null || target == context.Owner || !target.gameObject.activeInHierarchy)
        {
            evaluation = new AIUtilityEvaluation(new AIIntent(intentKey, null), false, 0f,
                "Attack infeasible: target is missing, inactive or the owner itself.");
            return true;
        }

        float distance = Vector2.Distance(context.Owner.position, target.position);
        AIIntent intent = new AIIntent(intentKey, target);
        if (distance > maxRange && !Mathf.Approximately(distance, maxRange))
        {
            evaluation = new AIUtilityEvaluation(intent, false, 0f,
                $"Attack infeasible: distance={distance:F4} exceeds maxRange={maxRange:F4}.");
            return true;
        }

        if (!context.FlightMotor.IsFacingTarget(target.position))
        {
            evaluation = new AIUtilityEvaluation(intent, false, 0f,
                "Attack infeasible: owner is facing away from target.");
            return true;
        }

        float normalizedDistance = Mathf.Clamp01(distance / maxRange);
        float curveScore = distanceUtilityCurve.Evaluate(normalizedDistance);
        if (float.IsNaN(curveScore) || float.IsInfinity(curveScore))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: distance curve returned a non-finite score at normalizedDistance={normalizedDistance}; attack evaluation failed.", this);
            return false;
        }

        float score = Mathf.Clamp01(curveScore);
        evaluation = new AIUtilityEvaluation(intent, true, score,
            $"Attack range and facing checks passed: distance={distance:F4}, maxRange={maxRange:F4}, normalizedDistance={normalizedDistance:F4}, curveScore={curveScore:F4}, finalScore={score:F4}. Other attack requirements are not evaluated yet.");
        return true;
    }
}
