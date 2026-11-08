using UnityEngine;

[CreateAssetMenu(fileName = "UtilityOption_Chase", menuName = "Game/AI/Utility/Chase Option")]
public class AIChaseUtilityOptionDefinition : AIUtilityOptionDefinition
{
    [SerializeField] private AIIntentKey intentKey;
    [SerializeField] private float minRange = 2f;
    [SerializeField] private float maxRange = 20f;
    [SerializeField] private AnimationCurve distanceUtilityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    public AIIntentKey IntentKey => intentKey;
    public float MinRange => minRange;
    public float MaxRange => maxRange;
    public AnimationCurve DistanceUtilityCurve => distanceUtilityCurve;

    public override bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation evaluation)
    {
        evaluation = default;
        if (context.Owner == null || intentKey == null)
        {
            Debug.LogWarning($"[AIUtility] {name}: owner or intent key is missing; chase evaluation failed.", this);
            return false;
        }

        if (!(minRange >= 0f && maxRange > minRange) ||
            float.IsInfinity(minRange) || float.IsInfinity(maxRange))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: invalid chase range ({minRange}, {maxRange}); expected finite 0 <= minRange < maxRange; evaluation failed.", this);
            return false;
        }

        if (distanceUtilityCurve == null || distanceUtilityCurve.length == 0)
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: distance utility curve is missing or empty; chase evaluation failed.", this);
            return false;
        }

        Transform target = context.Target;
        if (target == null || target == context.Owner || !target.gameObject.activeInHierarchy)
        {
            evaluation = new AIUtilityEvaluation(new AIIntent(intentKey, null), false, 0f,
                "Chase infeasible: target is missing, inactive or the owner itself.");
            return true;
        }

        float distance = Vector2.Distance(context.Owner.position, target.position);
        if (float.IsNaN(distance) || float.IsInfinity(distance))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: target distance is non-finite; chase evaluation failed.", this);
            return false;
        }

        AIIntent intent = new AIIntent(intentKey, target);
        if (distance <= minRange || Mathf.Approximately(distance, minRange))
        {
            evaluation = new AIUtilityEvaluation(intent, false, 0f,
                $"Chase infeasible: already close enough (distance={distance:F4}, minRange={minRange:F4}); no new chase needed.");
            return true;
        }

        if (distance > maxRange && !Mathf.Approximately(distance, maxRange))
        {
            evaluation = new AIUtilityEvaluation(intent, false, 0f,
                $"Chase infeasible: distance={distance:F4} exceeds maxRange={maxRange:F4}.");
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
            $"Chase range check passed: distance={distance:F4}, range=({minRange:F4}, {maxRange:F4}], normalizedDistance={normalizedDistance:F4}, curveScore={curveScore:F4}, finalScore={score:F4}. Path reachability is not evaluated yet.");
        return true;
    }
}
