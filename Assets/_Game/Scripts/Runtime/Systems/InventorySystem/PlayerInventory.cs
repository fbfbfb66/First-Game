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
        Debug.Log($"背包已创建：{grid.Width}*{grid.Height}");
    }

    private void Start()
    {
        // 测试：放置一个调试物品
        PlaceDebugItem(0, 0);
        PlaceDebugItem(1, 1);
        PlaceDebugItem(3, 6);
        PrintGrid();
    }

    public bool TryMove(InventoryItem item, int x, int y)
    {
        if (item == null) return false;
        if(CanPlace(item, x, y,true))
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

    private void PlaceDebugItem(int x, int y)
    {
        InventoryItem item = new InventoryItem(debugItem, 1);
        if (grid.Place(item, x, y))
        {
            ItemPlaced?.Invoke(item, x, y);
        }
    }

    private void PrintGrid()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"背包 {grid.Width}×{grid.Height}");

        for (int y = 0; y < grid.Height; y++)       // 外层是 y
        {
            for (int x = 0; x < grid.Width; x++)    // 内层是 x
            {
                InventoryItem item = grid.GetItemAt(x, y);
                sb.Append(item == null ? '.' : item.Data.DisplayName[0]);
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}
