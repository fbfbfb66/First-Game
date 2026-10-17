using UnityEngine;

[CreateAssetMenu(fileName = "HealItemEffect", menuName = "Game/Inventory/Heal Item Effect")]
public class HealItemEffect : ItemEffect
{
    [SerializeField, Min(0.01f)] private float healAmountPerItem = 25f;

    public float HealAmountPerItem => healAmountPerItem;

    public override bool TryApply(ItemEffectContext context, int amount)
    {
        if (context.source == null || context.itemData == null || amount <= 0) return false;

        float totalHealing = healAmountPerItem * amount;
        if (totalHealing <= 0f || float.IsNaN(totalHealing) || float.IsInfinity(totalHealing))
        {
            Debug.LogWarning($"{name} cannot apply healing: invalid total healing {totalHealing}. Effect rejected.", this);
            return false;
        }

        if (!context.source.TryGetComponent<ItemEffectReceiver>(out var receiver))
        {
            Debug.LogWarning($"{context.source.name} cannot use {context.itemData.DisplayName}: ItemEffectReceiver is missing. Effect rejected.", context.source);
            return false;
        }

        return receiver.TryHeal(totalHealing, context);
    }
}
