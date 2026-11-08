using System;
using UnityEngine;

/// <summary>
/// One runtime attribute: an immutable base value plus a mutable flat bonus.
/// </summary>
public sealed class Stat
{
    public float BaseValue { get; }
    public float FlatBonus { get; private set; }
    public float Value => BaseValue + FlatBonus;

    public Stat(float baseValue)
    {
        if (float.IsNaN(baseValue) || float.IsInfinity(baseValue))
            throw new ArgumentOutOfRangeException(nameof(baseValue), baseValue, "Base value must be finite.");

        BaseValue = baseValue;
    }

    public void AddFlatBonus(float amount)
    {
        float updatedBonus = FlatBonus + amount;
        float updatedValue = BaseValue + updatedBonus;
        if (float.IsNaN(updatedBonus) || float.IsInfinity(updatedBonus) || float.IsInfinity(updatedValue))
        {
            Debug.LogWarning($"Stat with base value {BaseValue} cannot add flat bonus {amount}: result must be finite. Stat unchanged.");
            return;
        }

        FlatBonus = updatedBonus;
    }

    /// <summary>
    /// Removes a previously added signed amount. The caller tracks its own contribution.
    /// </summary>
    public void RemoveFlatBonus(float amount)
    {
        AddFlatBonus(-amount);
    }
}
