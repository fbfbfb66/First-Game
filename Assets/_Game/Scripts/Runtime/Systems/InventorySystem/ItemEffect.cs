using UnityEngine;

/// <summary>
/// Defines an immediate item effect independently of inventory storage and UI.
/// </summary>
public abstract class ItemEffect : ScriptableObject
{
    /// <summary>
    /// Attempts to apply the effect for the requested number of items.
    /// Returns true when the effect succeeds; inventory consumption is handled by the caller.
    /// </summary>
    public abstract bool TryApply(ItemEffectContext context, int amount);
}
