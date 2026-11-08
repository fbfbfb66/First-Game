using System;

public readonly struct AIUtilityEvaluation
{
    public AIIntent Intent { get; }
    public bool IsFeasible { get; }
    public float UtilityScore { get; }
    public string Reason { get; }

    public AIUtilityEvaluation(AIIntent intent, bool isFeasible, float utilityScore, string reason)
    {
        if (intent.Key == null)
            throw new ArgumentException("Candidate intent must have a valid key.", nameof(intent));
        if (float.IsNaN(utilityScore) || float.IsInfinity(utilityScore))
            throw new ArgumentOutOfRangeException(nameof(utilityScore), utilityScore, "Utility score must be finite.");

        Intent = intent;
        IsFeasible = isFeasible;
        UtilityScore = utilityScore;
        Reason = reason ?? string.Empty;
    }
}
