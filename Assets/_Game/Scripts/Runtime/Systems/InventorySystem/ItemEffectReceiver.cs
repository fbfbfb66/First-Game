using UnityEngine;

/// <summary>
/// Receives item effects on their user. Healing is simulated until a health system exists.
/// </summary>
public class ItemEffectReceiver : MonoBehaviour
{
    public bool TryHeal(float amount, ItemEffectContext context)
    {
        if (context.source != gameObject || context.itemData == null) return false;
        if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return false;

        // Replace this simulation with the user's health component when it is introduced.
        Debug.Log($"[ItemEffect] {name} used {context.itemData.DisplayName}: simulated healing request = {amount} HP. No health value is changed yet.", this);
        return true;
    }
}
