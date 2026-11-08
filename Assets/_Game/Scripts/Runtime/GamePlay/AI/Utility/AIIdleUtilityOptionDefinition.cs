using UnityEngine;

[CreateAssetMenu(fileName = "UtilityOption_Idle", menuName = "Game/AI/Utility/Idle Option")]
public class AIIdleUtilityOptionDefinition : AIUtilityOptionDefinition
{
    [SerializeField] private AIIntentKey intentKey;
    [SerializeField] private float utilityScore;

    public AIIntentKey IntentKey => intentKey;
    public float UtilityScore => utilityScore;

    public override bool TryEvaluate(in AIAgentContext context, out AIUtilityEvaluation evaluation)
    {
        evaluation = default;
        if (context.Owner == null || intentKey == null)
        {
            Debug.LogWarning($"[AIUtility] {name}: owner or intent key is missing; idle evaluation failed.", this);
            return false;
        }

        if (float.IsNaN(utilityScore) || float.IsInfinity(utilityScore))
        {
            Debug.LogWarning($"[AIUtility] {name} for {context.Owner.name}: idle score must be finite; evaluation failed.", this);
            return false;
        }

        evaluation = new AIUtilityEvaluation(new AIIntent(intentKey, null), true, utilityScore,
            "Idle is available as a fallback; score uses the configured baseline.");
        return true;
    }
}
