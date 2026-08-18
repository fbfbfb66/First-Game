using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private RectTransform itemLayer;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private ItemView itemViewPrefab;
    [SerializeField] private float cellSize = 240f;
    [SerializeField] private float spacing = 5f;
    private Vector2Int hoveredCell = new Vector2Int(-1, -1);

    private void OnEnable()
    {
        if (inventory == null)
        {
            Debug.LogError("Inventory is not assigned in InventoryView.");
            return;
        }
        inventory.ItemPlaced += ShowItem;
    }

    private void OnDisable()
    {
        if (inventory == null)
            return;
        inventory.ItemPlaced -= ShowItem;
    }

    public void UpdateHover(Vector2 screenPosition)
    {
        if (TryGetCellAt(screenPosition, out int x, out int y))
        {
            if (hoveredCell.x != x || hoveredCell.y != y)
            {
                hoveredCell.x = x;
                hoveredCell.y = y;
                Debug.Log($"Mouse is over cell ({x}, {y}) : {inventory.GetItemAt(x, y)?.Data.DisplayName}");
            }
        }
        else
            ClearHover();
    }
    public void ClearHover()
    {
        hoveredCell = new Vector2Int(-1, -1);
    }

    public void ShowItem(InventoryItem item, int x, int y)
    {
        if (item == null || itemLayer == null || itemViewPrefab == null) return;

        ItemView view = Instantiate(itemViewPrefab, itemLayer);
        RectTransform rect = (RectTransform)view.transform;

        view.SetIcon(item);
        float step = cellSize + spacing;
        rect.anchoredPosition = new Vector2(x * step, -y * step);
        rect.sizeDelta = new Vector2(cellSize * item.CurrentWidth + (item.CurrentWidth - 1) * spacing, cellSize * item.CurrentHeight + (item.CurrentHeight - 1) * spacing);
    }

    public bool TryGetCellAt(Vector2 screenPosition,out int x,out int y)
    {
         x = -1;
         y = -1;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(itemLayer, screenPosition, null, out Vector2 localPosition)==false)
        {
            return false;
        }
        x = Mathf.FloorToInt(localPosition.x / (cellSize + spacing));
        y = Mathf.FloorToInt(-localPosition.y / (cellSize + spacing));
        if(inventory.IsInside(x, y))
        {
            return true;
        }
        return false;
    }
}
