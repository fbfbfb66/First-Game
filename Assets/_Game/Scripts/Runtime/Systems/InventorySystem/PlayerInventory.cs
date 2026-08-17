using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    //宽和高是根据现在场景里的背包写的
    [SerializeField] private int width = 4;
    [SerializeField] private int height = 8;

    [SerializeField] private ItemData debugItem;

    private InventoryGrid grid;

    private void Awake()
    {
        grid = new InventoryGrid(width, height);
        InventoryItem item = new InventoryItem(debugItem, 1);
        grid.Place(item, 0, 0);
        Debug.Log($"背包已创建：{grid.Width}*{grid.Height}");
        PrintGrid();
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
