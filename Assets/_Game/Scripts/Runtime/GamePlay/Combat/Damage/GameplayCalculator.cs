using UnityEngine;

/// <summary>
/// Gameplay calculations. Callers apply the returned results; an explicit roll enables deterministic calculations.
/// </summary>
public static class GameplayCalculator
{
    public static DamageResult CalculateOutgoingDamage(CharacterStats attackerStats, float damageMultiplier)
    {
        return CalculateOutgoingDamage(attackerStats, damageMultiplier, Random.value);
    }

    /// <summary>
    /// Calculates outgoing damage, including critical hits but excluding target defense.
    /// criticalRoll is supplied by the caller in [0, 1].
    /// </summary>
    public static DamageResult CalculateOutgoingDamage(
        CharacterStats attackerStats,
        float damageMultiplier,
        float criticalRoll)
    {
        if (attackerStats == null || !IsFinite(damageMultiplier) || damageMultiplier < 0f
            || !IsFinite(criticalRoll) || criticalRoll < 0f || criticalRoll > 1f)
        {
            Debug.LogWarning("Cannot calculate outgoing damage: stats, multiplier or critical roll is invalid. Returning zero damage.");
            return default;
        }

        float attack = Mathf.Max(0f, attackerStats.BaseAttack.Value);
        float criticalChance = Mathf.Clamp01(attackerStats.CriticalChance.Value);
        bool isCritical = criticalChance >= 1f || criticalRoll < criticalChance;
        float damage = attack * damageMultiplier;
        if (isCritical)
            damage *= Mathf.Max(1f, attackerStats.CriticalDamageMultiplier.Value);

        if (!IsFinite(damage))
        {
            Debug.LogWarning("Cannot calculate outgoing damage: result exceeds the supported numeric range. Returning zero damage.");
            return default;
        }

        return new DamageResult(damage, isCritical);
    }

    /// <summary>
    /// Applies the receiver's defense. Defense 100 reduces incoming damage by half.
    /// </summary>
    public static float CalculateIncomingDamage(CharacterStats targetStats, float incomingDamage)
    {
        if (targetStats == null || !IsFinite(incomingDamage) || incomingDamage < 0f)
        {
            Debug.LogWarning("Cannot calculate incoming damage: stats or incoming damage is invalid. Returning zero damage.");
            return 0f;
        }

        float defense = Mathf.Max(0f, targetStats.Defense.Value);
        return incomingDamage * (100f / (100f + defense));
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
