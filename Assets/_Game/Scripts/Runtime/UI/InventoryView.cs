using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] private RectTransform itemLayer;
    [SerializeField] private RectTransform dragLayer;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private ItemView itemViewPrefab;
    [SerializeField] private float cellSize = 240f;
    [SerializeField] private float spacing = 5f;

    private readonly Dictionary<InventoryItem, ItemView> itemViews = new();
    private Vector2Int hoveredCell = new Vector2Int(-1, -1);
    private InventoryItem hoveredItem = null;
    private InventoryItem dragItem;
    private Vector2 dragItemOriginalPosition;
    private Vector2 grabOffset;
    private bool allowToHeighlight = true;


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

    public void BeginDrag(Vector2 screenPosition)
    {
        if (TryGetCellAt(screenPosition, out int x, out int y))
        {
            dragItem = inventory.GetItemAt(x, y);
            if (dragItem != null)
            {
                if (itemViews.TryGetValue(dragItem, out var itemView))
                {
                    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screenPosition, null, out Vector2 localPosition))
                    {
                        RectTransform rect = (RectTransform)itemView.transform;
                        dragItemOriginalPosition = rect.anchoredPosition;
                        rect.SetParent(dragLayer, true);
                        grabOffset = localPosition - rect.anchoredPosition;
                        rect.anchoredPosition = localPosition - grabOffset;
                    }
                    itemView.SetBackgroundTransparent(true);
                    allowToHeighlight = false;
                }
                Debug.Log($"Begin dragging item: {dragItem.Data.DisplayName} from cell ({x}, {y})");
            }
        }
    }
    public void Drag(Vector2 screenPosition)
    {
        if (dragItem != null && itemViews.TryGetValue(dragItem, out var itemView))
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(dragLayer, screenPosition, null, out Vector2 localPosition))
            {
                RectTransform rect = (RectTransform)itemView.transform;
                rect.anchoredPosition = localPosition - grabOffset;
            }
        }
    }
    public void EndDrag()
    {
        if (dragItem != null && itemViews.TryGetValue(dragItem, out var itemView))
        {
            RectTransform rect = (RectTransform)itemView.transform;
            rect.SetParent(itemLayer, true);

            GetDropItemAt(rect.anchoredPosition, out int x, out int y);
            if (inventory.TryMove(dragItem, x, y))
            {
                rect.anchoredPosition = GetAnchorPositionForCell(x, y);
                Debug.Log($"Dropped item: {dragItem.Data.DisplayName} to cell ({x}, {y})");

            }
            else
                rect.anchoredPosition = dragItemOriginalPosition;

            itemView.SetBackgroundTransparent(false);
        }
        allowToHeighlight = true;
        dragItem = null;
    }

    public void UpdateHover(Vector2 screenPosition)
    {
        if (TryGetCellAt(screenPosition, out int x, out int y))
        {
            InventoryItem item = inventory.GetItemAt(x, y);
            if (hoveredCell.x != x || hoveredCell.y != y)
            {
                hoveredCell.x = x;
                hoveredCell.y = y;
                //Debug.Log($"Mouse is over cell ({x}, {y}) : {item?.Data.DisplayName}");
            }
            if (allowToHeighlight && hoveredItem != item)
            {
                if (hoveredItem != null && itemViews.TryGetValue(hoveredItem, out ItemView previousView))
                {
                    previousView.SetHighlighted(false);
                }
                hoveredItem = item;
                if (item != null && itemViews.TryGetValue(item, out ItemView view))
                {
                    view.SetHighlighted(true);
                }
            }
        }
        else
            ClearHover();
    }
    public void ClearHover()
    {
        hoveredCell = new Vector2Int(-1, -1);
        if (hoveredItem != null && itemViews.TryGetValue(hoveredItem, out ItemView view))
        {
            view.SetHighlighted(false);
            hoveredItem = null;
        }
    }

    public void ShowItem(InventoryItem item, int x, int y)
    {
        if (item == null || itemLayer == null || itemViewPrefab == null) return;

        ItemView view = Instantiate(itemViewPrefab, itemLayer);
        RectTransform rect = (RectTransform)view.transform;

        view.SetIcon(item);
        itemViews.Add(item, view);
        rect.anchoredPosition = GetAnchorPositionForCell(x, y);
        rect.sizeDelta = new Vector2(cellSize * item.CurrentWidth + (item.CurrentWidth - 1) * spacing, cellSize * item.CurrentHeight + (item.CurrentHeight - 1) * spacing);
    }

    public bool TryGetCellAt(Vector2 screenPosition, out int x, out int y)
    {
        x = -1;
        y = -1;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(itemLayer, screenPosition, null, out Vector2 localPosition) == false)
        {
            return false;
        }
        x = Mathf.FloorToInt(localPosition.x / (cellSize + spacing));
        y = Mathf.FloorToInt(-localPosition.y / (cellSize + spacing));
        if (inventory.IsInside(x, y))
        {
            return true;
        }
        return false;
    }

    public Vector2 GetAnchorPositionForCell(int x,int y)
    {
        float step = cellSize + spacing;
        return new Vector2(x * step, -y * step);
    }

    private void GetDropItemAt(Vector2 itemPosition, out int x, out int y)
    {
        x = Mathf.RoundToInt(itemPosition.x / (cellSize + spacing));
        y = Mathf.RoundToInt(-itemPosition.y / (cellSize + spacing));
    }

}
