using UnityEngine;

[CreateAssetMenu(fileName = "UtilityOption_Attack", menuName = "Game/AI/Enemies/Utility/Attack Option")]
public class AIAttackUtilityOptionDefinition : AIUtilityOptionDefinition
{
    [SerializeField] private AIIntentKey intentKey;
    [SerializeField] private CombatActionDefinition action;
    [SerializeField, Min(0f)] private float minRange;
    public float MinRange => minRange;
    [SerializeField] private float maxRange = 2f;
    [SerializeField] private AnimationCurve distanceUtilityCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    public AIIntentKey IntentKey => intentKey;
    public CombatActionDefinition Action => action;
    public float MaxRange => maxRange;
    public AnimationCurve DistanceUtilityCurve => distanceUtilityCurve;

    public override bool IsFeasible(in AIAgentContext context)
    {
        if (!base.IsFeasible(in context) || intentKey == null || action == null ||
            context.CombatExecutor == null || context.CombatExecutor.CombatActionController == null ||
            context.Motor == null) return false;

        if (context.CombatExecutor.CombatActionController.GetCooldownRemaining(action) > 0f) return false;

        Transform target = context.Target;
        if (target == null || target == context.Owner || !target.gameObject.activeInHierarchy) return false;

        float distance = Vector2.Distance(context.Owner.position, target.position);
        if (float.IsNaN(distance) || float.IsInfinity(distance)) return false;
        if ((distance < minRange && !Mathf.Approximately(distance, minRange)) ||
            (distance > maxRange && !Mathf.Approximately(distance, maxRange))) return false;

        return context.Motor.IsFacingTarget(target.position);
    }

    public override bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation evaluation)
    {
        evaluation = default;
        if (context.Owner == null || intentKey == null || action == null ||
            context.CombatExecutor == null || context.CombatExecutor.CombatActionController == null || context.Motor == null)
        {
            Debug.LogWarning($"[AIUtility] {name}: owner, intent key, action, Combat controller or facing motor is missing; attack evaluation failed.", this);
            return false;
        }

        if (!(minRange >= 0f && maxRange > minRange) || float.IsInfinity(minRange) || float.IsInfinity(maxRange))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: range=[{minRange},{maxRange}] must be finite with 0 <= min < max; attack evaluation failed.", this);
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

        AIIntent intent = new AIIntent(intentKey, context.Target, action);
        if (!IsFeasible(in context))
        {
            evaluation = new AIUtilityEvaluation(intent, false, 0f,
                "Attack infeasible: no detected target, owner or target is outside the active area, area is missing, or target is invalid, action is cooling down, or range/facing conditions are not met.");
            return true;
        }

        float distance = Vector2.Distance(context.Owner.position, context.Target.position);
        float normalizedDistance = Mathf.Clamp01((distance - minRange) / (maxRange - minRange));
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
