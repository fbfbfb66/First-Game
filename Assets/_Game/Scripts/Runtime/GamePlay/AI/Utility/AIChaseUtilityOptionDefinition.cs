using UnityEngine;

[CreateAssetMenu(fileName = "UtilityOption_Chase", menuName = "Game/AI/Utility/Chase Option")]
public class AIChaseUtilityOptionDefinition : AIUtilityOptionDefinition
{
    [SerializeField] private AIIntentKey intentKey;
    [SerializeField] private float minRange = 2f;
    [SerializeField] private AnimationCurve distanceUtilityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public AIIntentKey IntentKey => intentKey;
    public float MinRange => minRange;
    public AnimationCurve DistanceUtilityCurve => distanceUtilityCurve;

    public override bool IsFeasible(in AIAgentContext context)
    {
        if (!base.IsFeasible(in context) || intentKey == null || context.Motor == null) return false;

        Transform target = context.Target;
        if (target == null || target == context.Owner || !target.gameObject.activeInHierarchy) return false;

        float maxRange = context.Area.DiagonalLength;
        if (!(minRange >= 0f && maxRange > minRange) ||
            float.IsInfinity(minRange) || float.IsInfinity(maxRange)) return false;

        float distance = Vector2.Distance(context.Owner.position, target.position);
        if (float.IsNaN(distance) || float.IsInfinity(distance)) return false;

        if (distance <= minRange || Mathf.Approximately(distance, minRange)) return false;
        return distance <= maxRange || Mathf.Approximately(distance, maxRange);
    }

    public override bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation evaluation)
    {
        evaluation = default;
        if (context.Owner == null || intentKey == null)
        {
            Debug.LogWarning($"[AIUtility] {name}: owner or intent key is missing; chase evaluation failed.", this);
            return false;
        }

        float maxRange = context.Area != null ? context.Area.DiagonalLength : 0f;
        if (!(minRange >= 0f) || float.IsInfinity(minRange) ||
            (context.Area != null && (!(maxRange > minRange) || float.IsInfinity(maxRange))))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: invalid chase range ({minRange}, {maxRange}); expected finite 0 <= minRange < active area diagonal; evaluation failed.", this);
            return false;
        }

        if (distanceUtilityCurve == null || distanceUtilityCurve.length == 0)
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: distance utility curve is missing or empty; chase evaluation failed.", this);
            return false;
        }

        Transform target = context.Target;
        float distance = target != null ? Vector2.Distance(context.Owner.position, target.position) : 0f;
        if (float.IsNaN(distance) || float.IsInfinity(distance))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: target distance is non-finite; chase evaluation failed.", this);
            return false;
        }

        AIIntent intent = new AIIntent(intentKey, target);
        if (!IsFeasible(in context))
        {
            evaluation = new AIUtilityEvaluation(intent, false, 0f,
                "Chase infeasible: no detected target, owner or target is outside the active area, area/target/flight motor is missing, target is invalid, or distance conditions are not met.");
            return true;
        }

        float normalizedDistance = Mathf.Clamp01((distance - minRange) / (maxRange - minRange));
        float curveScore = distanceUtilityCurve.Evaluate(normalizedDistance);
        if (float.IsNaN(curveScore) || float.IsInfinity(curveScore))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: distance curve returned a non-finite score at normalizedDistance={normalizedDistance}; chase evaluation failed.", this);
            return false;
        }

        float score = Mathf.Clamp01(curveScore);
        evaluation = new AIUtilityEvaluation(intent, true, score,
            $"Chase range checks passed: distance={distance:F4}, range=({minRange:F4}, {maxRange:F4}], maxRange uses active area diagonal, normalizedDistance={normalizedDistance:F4}, curveScore={curveScore:F4}, finalScore={score:F4}. Path reachability is not evaluated yet.");
        return true;
    }
}
