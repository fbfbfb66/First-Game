using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    //宽和高是根据现在场景里的背包写的
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 8;

    [SerializeField] private ItemData debugItem;

    public event Action<InventoryItem, int, int> ItemPlaced;

    private InventoryGrid grid;

    private void Awake()
    {
        grid = new InventoryGrid(width, height);
    }

    public bool TryAdd(ItemData data, int amount = 1)
    {
        InventoryItem item = new InventoryItem(data, amount);
        if (grid.TryFindFreeCell(item, out int x, out int y))
        {
            grid.Place(item, x, y);
            ItemPlaced?.Invoke(item, x, y);
            return true;
        }
        return false;
    }

    public bool TryMove(InventoryItem item, int x, int y)
    {
        if (item == null) return false;
        if (CanPlace(item, x, y, true))
        {
            grid.Remove(item);
            grid.Place(item, x, y);
            return true;
        }
        return false;
    }
    public bool IsInside(int x, int y) => grid.IsInside(x, y);
    public bool CanPlace(InventoryItem item, int x, int y, bool ignoreItem = false) => grid.CanPlace(item, x, y, ignoreItem);
    public InventoryItem GetItemAt(int x, int y) => grid.GetItemAt(x, y);
}
