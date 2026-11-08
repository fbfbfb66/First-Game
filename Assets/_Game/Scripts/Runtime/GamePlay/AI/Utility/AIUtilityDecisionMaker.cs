using System.Collections.Generic;
using UnityEngine;

public class AIUtilityDecisionMaker
{
    public bool TrySelect(in AIAgentContext context, IReadOnlyList<AIUtilityOptionDefinition> options,
        out AIUtilityEvaluation selectedEvaluation)
    {
        selectedEvaluation = default;
        if (options == null || options.Count == 0)
        {
            Debug.LogWarning("[AIUtility] Option list is missing or empty; no intent selected.");
            return false;
        }

        bool hasSelection = false;
        for (int i = 0; i < options.Count; i++)
        {
            AIUtilityOptionDefinition definition = options[i];
            if (definition == null)
            {
                Debug.LogWarning($"[AIUtility] Candidate at index {i} is missing; entry skipped.");
                continue;
            }

            if (!definition.TryEvaluate(in context, out AIUtilityEvaluation evaluation)) continue;
            if (evaluation.Intent.Key == null || float.IsNaN(evaluation.UtilityScore) ||
                float.IsInfinity(evaluation.UtilityScore))
            {
                Debug.LogWarning($"[AIUtility] Candidate '{definition.name}' returned an invalid intent or score; entry skipped.", definition);
                continue;
            }

            if (!evaluation.IsFeasible) continue;
            // The first feasible result is valid even at zero or a negative score.
            // Strict comparison keeps the earlier entry on an exact tie.
            if (!hasSelection || evaluation.UtilityScore > selectedEvaluation.UtilityScore)
            {
                selectedEvaluation = evaluation;
                hasSelection = true;
            }
        }

        return hasSelection;
    }
}
