using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "Game/Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    [SerializeField] private ItemCategory category;
    [Range(1,4)]
    [SerializeField] private int width = 1;
    [Range(1,8)]
    [SerializeField] private int height = 1;
    [SerializeField] private int maxStack = 1;
    [SerializeField] private bool canRotate;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemCategory Category => category;
    public int Width => width;
    public int Height => height;
    public int MaxStack => maxStack;
    public bool CanRotate => canRotate;
}
