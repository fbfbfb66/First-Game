using UnityEngine;

public readonly struct ItemEffectContext
{
    public GameObject source { get; }
    public ItemData itemData { get; }
    public ItemEffectContext(GameObject source, ItemData itemData)
    {
        this.source = source;
        this.itemData = itemData;
    }
}
